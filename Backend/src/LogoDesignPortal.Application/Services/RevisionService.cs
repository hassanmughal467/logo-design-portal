using AutoMapper;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
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
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] VectorExtensions = { ".svg", ".pdf", ".ai", ".eps", ".psd" };
    private const long ImageMaxBytes = 10 * 1024 * 1024; // 10MB
    private const long VectorMaxBytes = 25 * 1024 * 1024; // 25MB

    public RevisionService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<RevisionService> logger,
        INotificationService notificationService,
        IRealtimeEntityUpdateSender entityUpdateSender)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
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

    private static long GetMaxSizeForExtension(string extension)
    {
        var ext = extension.ToLowerInvariant();
        if (ImageExtensions.Contains(ext)) return ImageMaxBytes;
        if (VectorExtensions.Contains(ext)) return VectorMaxBytes;
        return ImageMaxBytes;
    }

    private static void ValidateRevisionFile(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = ImageExtensions.Concat(VectorExtensions).Distinct().ToArray();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed for file '{file.FileName}'.");
        var maxSize = GetMaxSizeForExtension(ext);
        if (file.Length > maxSize)
            throw new InvalidOperationException($"File '{file.FileName}' exceeds maximum allowed size ({(ImageExtensions.Contains(ext) ? "10MB" : "25MB")}).");
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
        return order.Status == OrderStatus.PreviewDelivered;
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
            throw new InvalidOperationException("Revision can only be requested when order status is PreviewDelivered.");

        // Permanently delete all previous preview files
        await DeletePreviewFilesAsync(orderId);

        // Permanently delete all previous revision files
        await DeleteRevisionFilesAsync(orderId);

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
        order.Status = OrderStatus.RevisionRequested;
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
            var title = "Revision Requested";
            var message = $"{clientName} requested a revision for order (#{orderNumber})";
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId, requestedBy);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId, requestedBy);
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();
                if (designerUserId != Guid.Empty)
                {
                    await _notificationService.CreateNotificationAsync(designerUserId, title, message, NotificationType.RevisionRequest, orderId, NotificationReferenceType.Order, orderId, requestedBy);
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

        // Authorization
        if (userRole == "Client" && order.Client.UserId != userId)
            throw new ForbiddenAccessException("You don't have access to this order's revisions.");

        if (userRole == "Designer")
        {
            if (order.DesignerId == null)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");

            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");
        }

        // Only Admin and SuperAdmin can see revisions
        // Designers can see the latest revision for their assigned orders
        if (userRole != "Admin" && userRole != "SuperAdmin" && userRole != "Designer")
            return null;

        var latestRevision = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestRevision == null)
            return null;

        return await GetRevisionResponseAsync(latestRevision.Id);
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

        // Get all preview files (these will become final approved files)
        var previewFiles = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();

        // Get all revision files
        var revisionFiles = await _context.RevisionFiles
            .Include(rf => rf.Revision)
            .Where(rf => rf.Revision.OrderId == orderId && !rf.IsDeleted)
            .ToListAsync();

        // Permanently delete all revision files first
        foreach (var revisionFile in revisionFiles)
        {
            if (System.IO.File.Exists(revisionFile.FilePath))
            {
                System.IO.File.Delete(revisionFile.FilePath);
            }
            revisionFile.IsDeleted = true;
            revisionFile.DeletedAt = DateTime.UtcNow;
            revisionFile.DeletedBy = approvedBy;
        }

        // Convert preview files to final files and move to permanent storage
        foreach (var file in previewFiles)
        {
            // Move to permanent storage
            var permanentPath = Path.Combine(_permanentStoragePath, file.FileName);
            if (System.IO.File.Exists(file.FilePath))
            {
                // Ensure target directory exists
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
                PreviewImagePath = permanentPath, // For now, use same path
                ContentType = file.ContentType,
                Format = Path.GetExtension(file.OriginalFileName).TrimStart('.'),
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = approvedBy
            };

            _context.ClientGalleries.Add(galleryItem);
        }

        // Update order status and disable uploads for completed orders
        order.Status = OrderStatus.Completed;
        order.AllowUploads = false; // Disable uploads when logo is final approved
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = approvedBy;

        // Create audit log
        await CreateAuditLogAsync("ApproveLogo", "Order", orderId, approvedBy, 
            $"Logo approved. Notes: {request.Notes ?? "None"}");

        await _context.SaveChangesAsync();

        // Notify client: Order completed
        try
        {
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify client of order completion {OrderId}.", orderId);
        }

        // When client approves logo, notify Admin and Designer (PreviewApproved)
        if (userRole == "Client" && order.Client.UserId == approvedBy)
        {
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var approveTitle = "Preview Approved";
            var approveMessage = $"{clientName} approved the preview for order (#{orderNumber})";
            try
            {
                await _notificationService.CreateNotificationForRoleAsync("Admin", approveTitle, approveMessage, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId, approvedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", approveTitle, approveMessage, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId, approvedBy);
                if (order.DesignerId.HasValue)
                {
                    var designerUserId = await _context.DesignerProfiles
                        .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                        .Select(d => d.UserId)
                        .FirstOrDefaultAsync();
                    if (designerUserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(designerUserId, approveTitle, approveMessage, NotificationType.OrderStatusChange, orderId, NotificationReferenceType.Order, orderId, approvedBy);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create preview approved notifications for order {OrderId}.", orderId);
            }
        }

        // Real-time entity update: OrderStatusChanged (Completed) - notify client and admins
        var recipientIds = new List<Guid> { order.Client.UserId };
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (adminRole != null || superAdminRole != null)
        {
            var roleIds = new List<Guid>();
            if (adminRole != null) roleIds.Add(adminRole.Id);
            if (superAdminRole != null) roleIds.Add(superAdminRole.Id);
            var adminIds = await _context.Users
                .Where(u => !u.IsDeleted && roleIds.Contains(u.RoleId))
                .Select(u => u.Id)
                .ToListAsync();
            recipientIds.AddRange(adminIds);
        }
        if (order.DesignerId.HasValue)
        {
            var designerUserId = await _context.DesignerProfiles
                .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();
            if (designerUserId != Guid.Empty) recipientIds.Add(designerUserId);
        }
        await _entityUpdateSender.SendOrderStatusChangedAsync(orderId, OrderStatus.Completed.ToString(), approvedBy, recipientIds.Distinct());

        // When client approves logo, send PreviewApproved entity update to Admin/Designer
        if (userRole == "Client" && order.Client.UserId == approvedBy)
        {
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var adminIds = recipientIds.Where(id => id != approvedBy).ToList();
            if (adminIds.Any())
            {
                await _entityUpdateSender.SendPreviewApprovedAsync(orderId, clientName, OrderStatus.Completed.ToString(), adminIds);
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
    /// Deletes only designer-uploaded preview files. Never deletes Reference or Final files.
    /// Rule: FileType = Preview AND UploadedBy = Designer (for this order).
    /// </summary>
    private async Task DeletePreviewFilesAsync(Guid orderId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        List<LogoFile> previewFilesToDelete;
        if (order?.DesignerId == null)
        {
            previewFilesToDelete = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsDeleted)
                .ToListAsync();
        }
        else
        {
            var designerUserId = await _context.DesignerProfiles
                .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();

            previewFilesToDelete = await _context.LogoFiles
                .Where(f => f.OrderId == orderId
                    && f.FileType == FileType.Preview
                    && f.UploadedBy == designerUserId
                    && !f.IsDeleted)
                .ToListAsync();
        }

        foreach (var file in previewFilesToDelete)
        {
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
        }
        _context.LogoFiles.RemoveRange(previewFilesToDelete);
    }

    private async Task DeleteRevisionFilesAsync(Guid orderId)
    {
        var revisions = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .ToListAsync();

        foreach (var revision in revisions)
        {
            foreach (var file in revision.Files)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }
            }
            _context.RevisionFiles.RemoveRange(revision.Files);
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
