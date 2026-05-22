using AutoMapper;
using LogoDesignPortal.Application.Constants;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

public class RevisionService : IRevisionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RevisionService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly IInvoiceService _invoiceService;
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] VectorExtensions = { ".svg", ".pdf", ".ai", ".eps", ".psd" };
    private static readonly string[] EmbroiderExtensions = { ".pes", ".dst", ".jef", ".exp", ".vp3", ".xxx", ".hus", ".art", ".vip", ".vip3", ".shv", ".pec", ".jpm", ".sew", ".emb", ".csd", ".pcs", ".phb", ".phc", ".stx", ".s10", ".dsb", ".zsk" };
    public RevisionService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<RevisionService> logger,
        INotificationService notificationService,
        IRealtimeEntityUpdateSender entityUpdateSender,
        IInvoiceService invoiceService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _invoiceService = invoiceService;
        _fileStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        _temporaryStoragePath = Path.Combine(_fileStoragePath, "Temporary");
        _permanentStoragePath = Path.Combine(_fileStoragePath, "Permanent");
        
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        try
        {
            if (!Directory.Exists(_fileStoragePath))
                Directory.CreateDirectory(_fileStoragePath);
            if (!Directory.Exists(_temporaryStoragePath))
                Directory.CreateDirectory(_temporaryStoragePath);
            if (!Directory.Exists(_permanentStoragePath))
                Directory.CreateDirectory(_permanentStoragePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot create file storage directories at {Path}. Revision uploads will fail until write permissions are granted to the IIS App Pool identity.", _fileStoragePath);
        }
    }

    private static void ValidateRevisionFile(IFormFile file)
    {
        UploadSecurityHelper.ValidateUploadFileName(file.FileName);
        var ext = UploadSecurityHelper.GetEffectiveExtension(file.FileName);
        var allowed = ImageExtensions.Concat(VectorExtensions).Concat(EmbroiderExtensions).Distinct().ToArray();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed for file '{file.FileName}'.");
        if (file.Length > UploadLimits.MaxMultipartBytes)
            throw new InvalidOperationException($"File '{file.FileName}' exceeds maximum allowed size (500MB).");
        UploadSecurityHelper.ValidateDeclaredContentType(ext, file.ContentType);
        using var stream = file.OpenReadStream();
        UploadSecurityHelper.ValidateMagicBytes(ext, stream);
    }

    public async Task<bool> CanRequestRevisionAsync(Guid orderId, Guid userId, string userRole)
    {
        if (userRole != "Client")
            return false;

        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null || order.Client.UserId != userId)
            return false;

        // Allow revision only when status is PreviewDelivered
        if (order.Status != OrderStatus.PreviewDelivered)
            return false;

        // Check revision limit (unless admin approved extra revisions)
        var limit = order.RevisionLimit ?? RevisionLimitHelper.GetRevisionLimitFromPrice(order.Price);
        if (!RevisionLimitHelper.CanRequestRevision(order.RevisionCount, limit, order.AllowExtraRevisions))
            return false;

        return true;
    }

    public async Task<bool> CanApproveLogoAsync(Guid orderId, Guid userId, string userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            return false;

        // Allow approval when status is PreviewDelivered
        if (order.Status != OrderStatus.PreviewDelivered)
            return false;

        // Client can approve their own orders
        if (userRole == "Client")
        {
            return order.Client.UserId == userId;
        }

        // Admin and SuperAdmin can also approve
        if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            return true;
        }

        return false;
    }

    public async Task<RevisionResponseDto> RequestRevisionAsync(Guid orderId, RequestRevisionDto request, Guid requestedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (OrderLockingHelper.IsOrderLocked(order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        if (order.Client.UserId != requestedBy)
            throw new ForbiddenAccessException("You don't have permission to request revision for this order.");

        if (!await CanRequestRevisionAsync(orderId, requestedBy, "Client"))
        {
            var limit = order.RevisionLimit ?? RevisionLimitHelper.GetRevisionLimitFromPrice(order.Price);
            if (limit != null && order.RevisionCount >= limit.Value && !order.AllowExtraRevisions)
                throw new InvalidOperationException($"Revision limit ({limit}) exceeded for this order. Please contact support if you need additional revisions.");
            throw new InvalidOperationException("Revision can only be requested when order status is PreviewDelivered.");
        }

        // Increment revision count
        order.RevisionCount++;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = requestedBy;

        // Versioning: soft-delete previous preview files (preserve for audit/debugging). Never physically delete.
        await ArchivePreviewFilesAsync(orderId, requestedBy);

        // Versioning: soft-delete previous revision reference files (preserve for audit/debugging). Never physically delete.
        await ArchiveRevisionFilesAsync(orderId, requestedBy);

        // Archive previous revision instructions (soft delete, keep text)
        var previousRevisions = await _context.OrderRevisions
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .ToListAsync();

        foreach (var revision in previousRevisions)
        {
            revision.IsDeleted = true;
            revision.DeletedAt = DateTime.UtcNow;
            revision.DeletedBy = requestedBy;
        }

        // Create new revision
        var newRevision = new OrderRevision
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Instructions = request.Instructions,
            RequestedBy = requestedBy,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = requestedBy
        };

        _context.OrderRevisions.Add(newRevision);

        // Upload revision files if provided
        if (request.Files != null && request.Files.Length > 0)
        {
            long revisionFilesTotal = 0;
            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                    continue;
                revisionFilesTotal += file.Length;
            }

            if (revisionFilesTotal > UploadLimits.MaxMultipartBytes)
                throw new InvalidOperationException("Combined file size cannot exceed 500MB.");

            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                ValidateRevisionFile(file);

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_temporaryStoragePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var revisionFile = new RevisionFile
                {
                    Id = Guid.NewGuid(),
                    RevisionId = newRevision.Id,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileType = RevisionFileType.ReferenceImage,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = requestedBy
                };

                _context.RevisionFiles.Add(revisionFile);
            }
        }

        // Update order status
        OrderStatusTransitionHelper.Apply(order, OrderStatus.RevisionRequested);
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = requestedBy;

        _logger.LogInformation("RevisionRequested. OrderId={OrderId}, UserId={UserId}", orderId, requestedBy);

        // Create audit log
        await CreateAuditLogAsync("RequestRevision", "Order", orderId, requestedBy,
            $"Revision requested: {request.Instructions}");

        await _context.SaveChangesAsync();

        // Notify Admin and Designer: Client requested revision
        try
        {
            var clientName = await _context.Users
                .Where(u => u.Id == requestedBy)
                .Select(u => $"{u.FirstName} {u.LastName}".Trim())
                .FirstOrDefaultAsync() ?? "Client";
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var attachmentCount = request.Files?.Length ?? 0;
            var attachmentPart = attachmentCount > 0 ? $" [Revision] Includes {attachmentCount} reference file(s)." : string.Empty;
            var title = "[Revision] Revision requested";
            var note = (request.Instructions ?? string.Empty).Trim();
            if (note.Length > 180)
            {
                note = note.Substring(0, 180) + "...";
            }
            var messageStaff = string.IsNullOrWhiteSpace(note)
                ? $"[Revision] {clientName} requested a revision for order (#{orderNumber}).{attachmentPart}"
                : $"[Revision] {clientName} requested a revision for order (#{orderNumber}). Note: {note}{attachmentPart}";
            var messageDesigner = string.IsNullOrWhiteSpace(note)
                ? $"[Revision] A revision was requested for order (#{orderNumber}).{attachmentPart}"
                : $"[Revision] A revision was requested for order (#{orderNumber}). Note: {note}{attachmentPart}";
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, messageStaff, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId, requestedBy);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, messageStaff, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId, requestedBy);
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();
                if (designerUserId != Guid.Empty)
                {
                    await _notificationService.CreateNotificationAsync(designerUserId, title, messageDesigner, NotificationType.RevisionRequest, orderId, NotificationReferenceType.Order, orderId, requestedBy);
                }
            }
            var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
            var recipientIds = adminUserIds.ToList();
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();
                if (designerUserId != Guid.Empty)
                    recipientIds.Add(designerUserId);
            }
            await _entityUpdateSender.SendPreviewRejectedAsync(orderId, clientName, recipientIds.Distinct());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create preview rejected notifications for order {OrderId}.", orderId);
        }

        return await GetRevisionResponseAsync(newRevision.Id);
    }

    private async Task<List<Guid>> GetAdminAndSuperAdminUserIdsAsync()
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (adminRole == null && superAdminRole == null)
            return new List<Guid>();
        var roleIds = new List<Guid>();
        if (adminRole != null) roleIds.Add(adminRole.Id);
        if (superAdminRole != null) roleIds.Add(superAdminRole.Id);
        return await _context.Users
            .Where(u => !u.IsDeleted && roleIds.Contains(u.RoleId))
            .Select(u => u.Id)
            .ToListAsync();
    }

    public async Task<RevisionResponseDto?> GetLatestRevisionAsync(Guid orderId, Guid? userId, string? userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            return null;

        if (userRole == "Client")
        {
            if (order.Client?.UserId != userId)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");
        }
        else if (userRole == "Designer")
        {
            if (order.DesignerId == null)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");

            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");
        }
        else if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new ForbiddenAccessException("You don't have access to this order's revisions.");
        }

        var latestRevision = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestRevision == null)
            return null;

        return await GetRevisionResponseAsync(latestRevision.Id);
    }

    public async Task<(byte[] Content, string FileName, string ContentType)> DownloadRevisionFileAsync(Guid fileId, Guid userId, string? userRole)
    {
        var rf = await _context.RevisionFiles
            .AsNoTracking()
            .Include(f => f.Revision)
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

        if (rf?.Revision == null)
            throw new FileNotFoundException("Revision file not found.");

        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == rf.Revision.OrderId && !o.IsDeleted);

        if (order == null)
            throw new FileNotFoundException("Order not found.");

        if (userRole == "Client")
        {
            if (order.Client?.UserId != userId)
                throw new ForbiddenAccessException("You don't have access to this file.");
        }
        else if (userRole == "Designer")
        {
            if (order.DesignerId == null)
                throw new ForbiddenAccessException("You don't have access to this file.");

            var designer = await _context.DesignerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to this file.");
        }
        else if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new ForbiddenAccessException("You don't have access to this file.");
        }

        if (!System.IO.File.Exists(rf.FilePath))
            throw new FileNotFoundException("File is no longer available on disk.");

        var content = await System.IO.File.ReadAllBytesAsync(rf.FilePath);
        var contentType = string.IsNullOrWhiteSpace(rf.ContentType) ? "application/octet-stream" : rf.ContentType;
        return (content, rf.OriginalFileName, contentType);
    }

    public async Task<OrderResponseDto> ApproveLogoAsync(Guid orderId, ApproveLogoDto request, Guid approvedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        // Get the user to check their role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == approvedBy);

        var userRole = user?.Role?.Name ?? string.Empty;

        // Check permissions
        if (userRole == "Client")
        {
            if (order.Client.UserId != approvedBy)
                throw new ForbiddenAccessException("You don't have permission to approve this order.");
        }
        else if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new ForbiddenAccessException("You don't have permission to approve this order.");
        }

        if (!await CanApproveLogoAsync(orderId, approvedBy, userRole))
            throw new InvalidOperationException("Logo can only be approved when order status is PreviewDelivered.");

        // When Preview files are converted to Final: if pricing exists but not yet approved, trigger admin approval flow.
        // Do NOT block approval or completion; payout eligibility waits until price approval.
        if (order.ProposedPrice.HasValue && !DesignerPayoutPricingRules.HasFinalizedDesignerPayout(order))
        {
            order.PriceApprovalStatus = PriceApprovalStatus.PendingApproval;
            order.RequiresPriceApproval = true;
            try
            {
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Designer Price Approval Needed";
                var message = $"Preview converted to Final for order (#{orderNumber}). Designer proposed PKR {order.ProposedPrice:N0}. Approve to add to designer invoice.";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, approvedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, approvedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Designer Price Approval Needed notification for order {OrderId}.", orderId);
            }
        }

        // Step 1 — Approved files = FileType==Preview AND IsVisibleToClient==true AND IsAdminApproved==true (delivered to client)

        // Step 2 — Convert ONLY approved preview files (IsVisibleToClient==true) to Final
        var filesToConvert = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && f.IsVisibleToClient && !f.IsDeleted)
            .ToListAsync();

        foreach (var file in filesToConvert)
        {
            // Move from Temporary/ to Permanent/
            var permanentPath = Path.Combine(_permanentStoragePath, file.FileName);
            if (System.IO.File.Exists(file.FilePath))
            {
                var targetDir = Path.GetDirectoryName(permanentPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir!);
                }

                System.IO.File.Move(file.FilePath, permanentPath, overwrite: true);
                file.FilePath = permanentPath;
            }

            // Update file to Final type
            file.FileType = FileType.Final;
            file.IsFinalVersion = true;
            file.IsVisibleToClient = true;
            file.IsAdminApproved = true;
            file.ApprovedBy = approvedBy;
            file.ApprovedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;
            file.UpdatedBy = approvedBy;

            // Add to client gallery
            var galleryItem = new ClientGallery
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ClientId = order.ClientId,
                FileId = file.Id,
                FileName = file.FileName,
                OriginalFileName = file.OriginalFileName,
                FilePath = permanentPath,
                PreviewImagePath = permanentPath,
                ContentType = file.ContentType,
                Format = Path.GetExtension(file.OriginalFileName).TrimStart('.'),
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = approvedBy
            };

            _context.ClientGalleries.Add(galleryItem);
        }

        // When CLIENT approves: set ClientApproved so Admin can mark Completed. Admin/SuperAdmin get notification only (not client portal).
        // When ADMIN/SuperAdmin approves: set Completed directly, notify Client and Designer.
        if (order.Client == null)
            throw new InvalidOperationException("Order has no associated client.");
        var isClientApproval = userRole == "Client" && order.Client.UserId == approvedBy;
        OrderStatusTransitionHelper.Apply(order, isClientApproval ? OrderStatus.ClientApproved : OrderStatus.Completed);
        order.AllowUploads = false;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = approvedBy;

        if (!isClientApproval)
        {
            order.BillingEligible = true;
            order.IsInvoiced = false;
            order.CompletedDate = DateTime.UtcNow;
        }

        // Create audit log
        await CreateAuditLogAsync("ApproveLogo", "Order", orderId, approvedBy,
            $"Logo approved. Notes: {request.Notes ?? "None"}");

        await _context.SaveChangesAsync();

        // Step 3 & 4 — Safe file cleanup: only after conversion succeeded and status is ClientApproved/Completed
        var finalStatus = isClientApproval ? OrderStatus.ClientApproved : OrderStatus.Completed;
        if (finalStatus == OrderStatus.ClientApproved || finalStatus == OrderStatus.Completed)
        {
            await CleanupAfterApprovalAsync(orderId, approvedBy);
        }

        if (isClientApproval)
        {
            // Client approved final logo: bell notification for Admin/SuperAdmin only (admin marks Completed next).
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var approveTitle = "Client Approved Logo";
            var approveMessage = $"{clientName} approved the logo for order (#{orderNumber}). Please mark as completed.";
            try
            {
                await _notificationService.CreateNotificationForRoleAsync("Admin", approveTitle, approveMessage, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId, approvedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", approveTitle, approveMessage, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId, approvedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create client-approved notifications for order {OrderId}.", orderId);
            }

            // Real-time grid sync: Admin/SuperAdmin and assigned designer (no designer bell notification).
            var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
            var recipients = adminUserIds.ToList();
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();
                if (designerUserId != Guid.Empty)
                    recipients.Add(designerUserId);
            }
            var distinctRecipients = recipients.Distinct().ToList();
            await _entityUpdateSender.SendPreviewApprovedAsync(orderId, clientName, OrderStatus.ClientApproved.ToString(), distinctRecipients);
            await _entityUpdateSender.SendOrderStatusChangedAsync(orderId, OrderStatus.ClientApproved.ToString(), approvedBy, distinctRecipients);
        }
        else
        {
            // Admin/SuperAdmin approved (marked completed): notify Client and Designer
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            try
            {
                await _notificationService.CreateNotificationAsync(
                    order.Client.UserId,
                    "Order Completed",
                    $"Your order (#{orderNumber}) has been completed",
                    NotificationType.OrderStatusChange,
                    orderId,
                    NotificationReferenceType.Order,
                    orderId,
                    approvedBy
                );
                if (order.DesignerId.HasValue)
                {
                    var designerUserId = await _context.DesignerProfiles
                        .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                        .Select(d => d.UserId)
                        .FirstOrDefaultAsync();
                    if (designerUserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(designerUserId, "Order Completed", $"Order (#{orderNumber}) has been completed", NotificationType.OrderStatusChange, orderId, NotificationReferenceType.Order, orderId, approvedBy);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify client/designer of order completion {OrderId}.", orderId);
            }

            var recipientIds = new List<Guid> { order.Client.UserId };
            recipientIds.AddRange(await GetAdminAndSuperAdminUserIdsAsync());
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();
                if (designerUserId != Guid.Empty) recipientIds.Add(designerUserId);
            }
            await _entityUpdateSender.SendOrderStatusChangedAsync(orderId, OrderStatus.Completed.ToString(), approvedBy, recipientIds.Distinct());

            // Auto-generate invoice only for PerLogo clients when Admin approves (marks Completed)
            if (order.Client != null && order.Client.BillingType == Domain.Enums.BillingType.PerLogo)
            {
                try
                {
                    if (!order.IsInvoiced)
                    {
                        await _invoiceService.CreateInvoiceAsync(
                            new CreateInvoiceRequestDto { OrderIds = new List<Guid> { orderId }, BillingType = Domain.Enums.BillingType.PerLogo },
                            approvedBy);
                        _logger.LogInformation("Auto-generated invoice for PerLogo client, completed order {OrderId}.", orderId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-generate invoice for completed order {OrderId}. Invoice can be created manually.", orderId);
                }
            }
        }

        // Return order response using mapper
        var updatedOrder = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        return _mapper.Map<OrderResponseDto>(updatedOrder);
    }

    /// <summary>
    /// Safe file cleanup after approval: deletes non-approved preview files and all revision files.
    /// Only runs when order status is ClientApproved or Completed. Deletes physical files and soft-deletes DB rows.
    /// </summary>
    private async Task CleanupAfterApprovalAsync(Guid orderId, Guid approvedBy)
    {
        // Delete preview files where FileType==Preview AND IsVisibleToClient==false (not delivered to client)
        var nonApprovedPreviews = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsVisibleToClient && !f.IsDeleted)
            .ToListAsync();

        foreach (var file in nonApprovedPreviews)
        {
            if (System.IO.File.Exists(file.FilePath))
            {
                try
                {
                    System.IO.File.Delete(file.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete non-approved preview file {FilePath} for order {OrderId}.", file.FilePath, orderId);
                }
            }
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            file.DeletedBy = approvedBy;
        }

        // Delete all RevisionFiles for the order (physical + soft-delete)
        var revisionFiles = await _context.RevisionFiles
            .Include(rf => rf.Revision)
            .Where(rf => rf.Revision.OrderId == orderId && !rf.IsDeleted)
            .ToListAsync();

        foreach (var revisionFile in revisionFiles)
        {
            if (System.IO.File.Exists(revisionFile.FilePath))
            {
                try
                {
                    System.IO.File.Delete(revisionFile.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete revision file {FilePath} for order {OrderId}.", revisionFile.FilePath, orderId);
                }
            }
            revisionFile.IsDeleted = true;
            revisionFile.DeletedAt = DateTime.UtcNow;
            revisionFile.DeletedBy = approvedBy;
        }

        if (nonApprovedPreviews.Count > 0 || revisionFiles.Count > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("CleanupAfterApproval. OrderId={OrderId}, DeletedPreviews={PreviewCount}, DeletedRevisionFiles={RevisionCount}",
                orderId, nonApprovedPreviews.Count, revisionFiles.Count);
        }
    }

    /// <summary>
    /// Archives (soft-deletes) previous preview files. Never physically deletes or moves files.
    /// Preserves file history for audit and debugging. Archived previews remain IsVisibleToClient = false.
    /// </summary>
    private async Task ArchivePreviewFilesAsync(Guid orderId, Guid requestedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        List<LogoFile> previewFilesToArchive;
        if (order?.DesignerId == null)
        {
            previewFilesToArchive = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsDeleted)
                .ToListAsync();
        }
        else
        {
            var designerUserId = await _context.DesignerProfiles
                .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();

            previewFilesToArchive = await _context.LogoFiles
                .Where(f => f.OrderId == orderId
                    && f.FileType == FileType.Preview
                    && f.UploadedBy == designerUserId
                    && !f.IsDeleted)
                .ToListAsync();
        }

        foreach (var file in previewFilesToArchive)
        {
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            file.DeletedBy = requestedBy;
            file.IsVisibleToClient = false; // Keep archived previews hidden from client
        }
    }

    /// <summary>
    /// Archives (soft-deletes) previous revision reference files. Never physically deletes or moves files.
    /// Preserves file history for audit and debugging.
    /// </summary>
    private async Task ArchiveRevisionFilesAsync(Guid orderId, Guid requestedBy)
    {
        var revisions = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .ToListAsync();

        foreach (var revision in revisions)
        {
            foreach (var file in revision.Files.Where(f => !f.IsDeleted))
            {
                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;
                file.DeletedBy = requestedBy;
            }
        }
    }

    private async Task<RevisionResponseDto> GetRevisionResponseAsync(Guid revisionId)
    {
        var revision = await _context.OrderRevisions
            .Include(r => r.Files.Where(f => !f.IsDeleted))
            .Include(r => r.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null)
            throw new InvalidOperationException("Revision not found.");

        var requestedByUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == revision.RequestedBy);

        return new RevisionResponseDto
        {
            Id = revision.Id,
            OrderId = revision.OrderId,
            Instructions = revision.Instructions,
            RequestedBy = revision.RequestedBy,
            RequestedByName = requestedByUser != null 
                ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" 
                : string.Empty,
            IsResolved = revision.IsResolved,
            ResolvedAt = revision.ResolvedAt,
            CreatedAt = revision.CreatedAt,
            Files = revision.Files.Select(f => new RevisionFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                OriginalFileName = f.OriginalFileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                CreatedAt = f.CreatedAt
            }).ToList()
        };
    }

    private async Task CreateAuditLogAsync(string action, string entityType, Guid entityId, Guid userId, string details)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedByUserId = userId,
            PerformedByRole = user?.Role?.Name,
            Notes = details,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.AuditLogs.Add(auditLog);
    }
}
