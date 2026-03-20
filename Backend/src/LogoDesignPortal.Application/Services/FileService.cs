using AutoMapper;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Common;
using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Application.DTOs.Files;
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
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Services;

public class FileService : IFileService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<FileService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly IDesignerPayoutService _designerPayoutService;
    private readonly ProductionSafetyOptions _safetyOptions;
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] VectorExtensions = { ".svg", ".pdf", ".ai", ".eps", ".psd" };
    private static readonly string[] EmbroiderExtensions = { ".pes", ".dst", ".jef", ".exp", ".vp3", ".xxx", ".hus", ".art", ".vip", ".vip3", ".shv", ".pec", ".jpm", ".sew", ".emb", ".csd", ".pcs", ".phb", ".phc", ".stx", ".s10", ".dsb", ".zsk" };
    private const long ImageMaxBytes = 10 * 1024 * 1024; // 10MB
    private const long VectorMaxBytes = 25 * 1024 * 1024; // 25MB

    public FileService(IApplicationDbContext context, IConfiguration configuration, IMapper mapper, ILogger<FileService> logger, INotificationService notificationService, IRealtimeEntityUpdateSender entityUpdateSender, IDesignerPayoutService designerPayoutService, IOptions<ProductionSafetyOptions> safetyOptions)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _designerPayoutService = designerPayoutService;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
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
            _logger.LogWarning(ex, "Cannot create file storage directories at {Path}. File uploads will fail until write permissions are granted to the IIS App Pool identity (e.g. IIS AppPool\\HawkBE) for this path.", _fileStoragePath);
        }
    }

    private static long GetMaxSizeForExtension(string extension)
    {
        var ext = extension.ToLowerInvariant();
        if (ImageExtensions.Contains(ext)) return ImageMaxBytes;
        if (VectorExtensions.Contains(ext)) return VectorMaxBytes;
        if (EmbroiderExtensions.Contains(ext)) return VectorMaxBytes;
        return ImageMaxBytes; // default for unknown
    }

    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        [".gif"] = new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } },
        [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } },
        [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        [".svg"] = new[] { new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, new byte[] { 0x3C, 0x73, 0x76, 0x67 } }
    };

    private static void ValidateFile(IFormFile file, string? fileNameForError = null)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = ImageExtensions.Concat(VectorExtensions).Concat(EmbroiderExtensions).Distinct().ToArray();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed{(fileNameForError != null ? $" for file '{fileNameForError}'" : ".")}");
        var maxSize = GetMaxSizeForExtension(ext);
        if (file.Length > maxSize)
            throw new InvalidOperationException($"File {(fileNameForError != null ? $"'{fileNameForError}'" : "")} exceeds maximum allowed size ({(ImageExtensions.Contains(ext) ? "10MB" : "25MB")}).");
        if (MagicBytes.TryGetValue(ext, out var signatures))
        {
            using var stream = file.OpenReadStream();
            var header = new byte[Math.Max(8, signatures.Max(s => s.Length))];
            var read = stream.Read(header, 0, header.Length);
            if (read < signatures.Min(s => s.Length))
                throw new InvalidOperationException($"File content does not match extension '{ext}'.");
            var matches = signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
            if (!matches && ext == ".webp")
            {
                var riff = header.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 });
                var webp = read >= 12 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
                matches = riff && webp;
            }
            if (!matches)
                throw new InvalidOperationException($"File content does not match extension '{ext}'. Possible file type spoofing.");
        }
    }

    public async Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy, string fileType, string? description = null, int? designCategory = null, int? designType = null, decimal? proposedPrice = null)
    {
        if (_safetyOptions.DisableFileUploads)
            throw new InvalidOperationException("File uploads temporarily disabled by administrator.");

        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        await VerifyOrderUploadAccessAsync(order, uploadedBy);

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        // Check if uploads are allowed for this order
        if (!order.AllowUploads)
        {
            throw new InvalidOperationException("File uploads are disabled for this order. Please contact an administrator to enable uploads.");
        }

        ValidateFile(file);

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == uploadedBy);
        var userRole = user?.Role?.Name ?? string.Empty;

        // Parse file type enum first (needed for pricing and storage logic)
        if (!Enum.TryParse<FileType>(fileType, true, out var parsedFileType))
        {
            parsedFileType = FileType.Reference;
        }

        // When designer uploads first Preview batch and order has no pricing yet, require and submit design pricing.
        // This ensures pricing exists when Preview→Final conversion happens (client approves) without designer ever uploading Final.
        if (parsedFileType == FileType.Preview && userRole == "Designer" && !order.ProposedPrice.HasValue)
        {
            if (!designCategory.HasValue || !designType.HasValue || !proposedPrice.HasValue || proposedPrice.Value <= 0)
                throw new InvalidOperationException("Design category, design type, and proposed price are required when uploading the first preview batch.");
            if (!Enum.IsDefined(typeof(DesignCategory), designCategory.Value) || !Enum.IsDefined(typeof(DesignType), designType.Value))
                throw new InvalidOperationException("Invalid design category or design type.");
            await _designerPayoutService.SubmitDesignerPricingAsync(orderId, new SubmitDesignerPricingRequestDto
            {
                DesignCategory = (DesignCategory)designCategory.Value,
                DesignType = (DesignType)designType.Value,
                ProposedPrice = proposedPrice.Value
            }, uploadedBy);
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        
        // Determine storage path based on file type
        // Preview and Revision files go to temporary storage
        // Final files go to permanent storage
        // Reference files go to main storage (client uploads)
        string storagePath;
        if (parsedFileType == FileType.Preview || parsedFileType == FileType.Revision)
        {
            storagePath = _temporaryStoragePath;
        }
        else if (parsedFileType == FileType.Final)
        {
            storagePath = _permanentStoragePath;
        }
        else
        {
            storagePath = _fileStoragePath;
        }
        
        var filePath = Path.Combine(storagePath, fileName);

        // File system safety: validate storage path is within allowed base
        var fullBasePath = Path.GetFullPath(_fileStoragePath);
        var fullFilePath = Path.GetFullPath(filePath);
        if (!fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("FileSystemSafety: Rejected path outside storage base. Path={Path}", filePath);
            throw new InvalidOperationException("Invalid file storage path. Please contact support.");
        }

        // When designer uploads Final files, require and submit design pricing (only if not already submitted via Preview)
        if (parsedFileType == FileType.Final && userRole == "Designer" && !order.ProposedPrice.HasValue)
        {
            if (!designCategory.HasValue || !designType.HasValue || !proposedPrice.HasValue || proposedPrice.Value <= 0)
                throw new InvalidOperationException("Design category, design type, and proposed price are required when uploading final files.");
            if (!Enum.IsDefined(typeof(DesignCategory), designCategory.Value) || !Enum.IsDefined(typeof(DesignType), designType.Value))
                throw new InvalidOperationException("Invalid design category or design type.");
            await _designerPayoutService.SubmitDesignerPricingAsync(orderId, new SubmitDesignerPricingRequestDto
            {
                DesignCategory = (DesignCategory)designCategory.Value,
                DesignType = (DesignType)designType.Value,
                ProposedPrice = proposedPrice.Value
            }, uploadedBy);
        }

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage failed for order {OrderId}. Path={Path}", orderId, filePath);
            throw new InvalidOperationException("File storage failed. Please try again or contact support.");
        }

        // Version = 0 for client reference files (until designer sends via Super Admin); delivery round for Designer Preview; per-file sequence for others
        int versionNumber;
        if (userRole == "Client" && parsedFileType == FileType.Reference)
        {
            versionNumber = 0;
        }
        else if (parsedFileType == FileType.Preview && userRole == "Designer")
        {
            var existingBatchCount = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && f.PreviewBatchId != null && !f.IsDeleted)
                .Select(f => f.PreviewBatchId)
                .Distinct()
                .CountAsync();
            versionNumber = existingBatchCount + 1;
        }
        else
        {
            var existingFiles = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && !f.IsDeleted)
                .ToListAsync();
            versionNumber = existingFiles.Any() ? existingFiles.Max(f => f.VersionNumber) + 1 : 1;
        }
        
        // Fixed: Designer uploads are NEVER visible to clients until admin approval
        // Only client uploads of Reference type are visible immediately
        var isVisibleToClient = userRole == "Client" && parsedFileType == FileType.Reference;
        var isAdminApproved = userRole == "Client" && parsedFileType == FileType.Reference;

        var previewBatchId = (userRole == "Designer" && parsedFileType == FileType.Preview) ? Guid.NewGuid() : (Guid?)null;

        var logoFile = new LogoFile
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = filePath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileType = parsedFileType,
            VersionNumber = versionNumber,
            Description = description,
            IsVisibleToClient = isVisibleToClient,
            IsAdminApproved = isAdminApproved,
            IsFinalVersion = parsedFileType == FileType.Final,
            UploadedBy = uploadedBy,
            PreviewBatchId = previewBatchId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };

        _context.LogoFiles.Add(logoFile);

        // Designer uploads do NOT change order status to PreviewDelivered.
        // Status changes to PreviewDelivered only when admin forwards/approves files via SendFilesToClientAsync.
        // This ensures client gets "Action Required" only when they can actually see and review the files.

        await _context.SaveChangesAsync();

        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            _logger.LogInformation("PreviewUploaded. OrderId={OrderId}, UserId={UserId}, FileId={FileId}", orderId, uploadedBy, logoFile.Id);
        }

        // When designer uploads preview files, notify Admin/SuperAdmin (SignalR + toast/notification bell)
        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            try
            {
                var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                await _entityUpdateSender.SendPreviewUploadedAsync(orderId, adminUserIds);

                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Preview Ready for QA";
                var message = $"Designer uploaded preview files for order (#{orderNumber})";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send PreviewUploaded notification for order {OrderId}.", orderId);
            }
        }

        // When designer uploads final files, notify Admin/SuperAdmin
        if (userRole == "Designer" && parsedFileType == FileType.Final)
        {
            try
            {
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Final Files Uploaded";
                var message = $"Designer uploaded final files for order (#{orderNumber}). Review and approve designer price if needed.";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send FinalUploaded notification for order {OrderId}.", orderId);
            }
        }

        // When client uploads reference files, notify Admin and SuperAdmin
        if (userRole == "Client" && parsedFileType == FileType.Reference)
        {
            try
            {
                var orderWithClient = await _context.LogoOrders
                    .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
                if (orderWithClient?.Client != null)
                {
                    var clientName = $"{orderWithClient.Client.User.FirstName} {orderWithClient.Client.User.LastName}".Trim();
                    if (string.IsNullOrEmpty(clientName)) clientName = "Client";
                    var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                    var title = "File Uploaded by Client";
                    var message = $"{clientName} uploaded files for order (#{orderNumber})";
                    await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                    await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create file upload notification for order {OrderId}.", orderId);
            }
        }

        return new FileUploadResponseDto
        {
            Id = logoFile.Id,
            FileName = logoFile.FileName,
            OriginalFileName = logoFile.OriginalFileName,
            FileSize = logoFile.FileSize,
            ContentType = logoFile.ContentType,
            UploadedAt = logoFile.CreatedAt
        };
    }

    /// <inheritdoc />
    public async Task<List<LogoFile>> PrepareReferenceFilesForOrderAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string? description = null)
    {
        if (files == null || files.Length == 0)
            throw new InvalidOperationException("No files provided.");

        var results = new List<LogoFile>();
        var storagePath = _fileStoragePath; // Reference files go to main storage
        // Client reference files are version 0 until designer sends files via Super Admin
        const int clientReferenceVersion = 0;

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;

            ValidateFile(file, file.FileName);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var logoFile = new LogoFile
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                FileName = fileName,
                OriginalFileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileType = FileType.Reference,
                VersionNumber = clientReferenceVersion,
                Description = description,
                IsVisibleToClient = true,
                IsAdminApproved = true,
                IsFinalVersion = false,
                UploadedBy = uploadedBy,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = uploadedBy
            };

            results.Add(logoFile);
        }

        return results;
    }

    public async Task<List<FileUploadResponseDto>> UploadMultipleFilesAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string fileType, string? description = null, int? designCategory = null, int? designType = null, decimal? proposedPrice = null)
    {
        if (_safetyOptions.DisableFileUploads)
            throw new InvalidOperationException("File uploads temporarily disabled by administrator.");

        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        await VerifyOrderUploadAccessAsync(order, uploadedBy);

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        // Check if uploads are allowed for this order
        if (!order.AllowUploads)
        {
            throw new InvalidOperationException("File uploads are disabled for this order. Please contact an administrator to enable uploads.");
        }

        if (files == null || files.Length == 0)
        {
            throw new InvalidOperationException("No files provided.");
        }

        var results = new List<FileUploadResponseDto>();

        // Parse file type enum
        if (!Enum.TryParse<FileType>(fileType, true, out var parsedFileType))
        {
            parsedFileType = FileType.Reference;
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == uploadedBy);
        var userRole = user?.Role?.Name ?? string.Empty;

        // When designer uploads first Preview batch and order has no pricing yet, require and submit design pricing.
        // This ensures pricing exists when Preview→Final conversion happens (client approves) without designer ever uploading Final.
        if (parsedFileType == FileType.Preview && userRole == "Designer" && !order.ProposedPrice.HasValue)
        {
            if (!designCategory.HasValue || !designType.HasValue || !proposedPrice.HasValue || proposedPrice.Value <= 0)
                throw new InvalidOperationException("Design category, design type, and proposed price are required when uploading the first preview batch.");
            if (!Enum.IsDefined(typeof(DesignCategory), designCategory.Value) || !Enum.IsDefined(typeof(DesignType), designType.Value))
                throw new InvalidOperationException("Invalid design category or design type.");
            await _designerPayoutService.SubmitDesignerPricingAsync(orderId, new SubmitDesignerPricingRequestDto
            {
                DesignCategory = (DesignCategory)designCategory.Value,
                DesignType = (DesignType)designType.Value,
                ProposedPrice = proposedPrice.Value
            }, uploadedBy);
        }

        // When designer uploads Final files, require and submit design pricing (only if not already submitted via Preview)
        if (parsedFileType == FileType.Final && userRole == "Designer" && !order.ProposedPrice.HasValue)
        {
            if (!designCategory.HasValue || !designType.HasValue || !proposedPrice.HasValue || proposedPrice.Value <= 0)
                throw new InvalidOperationException("Design category, design type, and proposed price are required when uploading final files.");
            if (!Enum.IsDefined(typeof(DesignCategory), designCategory.Value) || !Enum.IsDefined(typeof(DesignType), designType.Value))
                throw new InvalidOperationException("Invalid design category or design type.");
            await _designerPayoutService.SubmitDesignerPricingAsync(orderId, new SubmitDesignerPricingRequestDto
            {
                DesignCategory = (DesignCategory)designCategory.Value,
                DesignType = (DesignType)designType.Value,
                ProposedPrice = proposedPrice.Value
            }, uploadedBy);
        }

        // Each designer preview upload session gets a new PreviewBatchId
        var previewBatchId = (userRole == "Designer" && parsedFileType == FileType.Preview) ? Guid.NewGuid() : (Guid?)null;

        // Version = delivery round for Designer Preview (all files in batch get same version), per-file sequence for others
        int currentVersion;
        if (parsedFileType == FileType.Preview && userRole == "Designer")
        {
            var existingBatchCount = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && f.PreviewBatchId != null && !f.IsDeleted)
                .Select(f => f.PreviewBatchId)
                .Distinct()
                .CountAsync();
            currentVersion = existingBatchCount; // Will use same version for all files in loop (no increment)
        }
        else
        {
            var existingFiles = await _context.LogoFiles
                .Where(f => f.OrderId == orderId && !f.IsDeleted)
                .ToListAsync();
            currentVersion = existingFiles.Any() ? existingFiles.Max(f => f.VersionNumber) : 0;
        }
        
        // Fixed: Designer uploads are NEVER visible to clients until admin approval
        // Only client uploads of Reference type are visible immediately
        var isVisibleToClient = userRole == "Client" && parsedFileType == FileType.Reference;
        var isAdminApproved = userRole == "Client" && parsedFileType == FileType.Reference;

        // Determine storage path based on file type
        string storagePath;
        if (parsedFileType == FileType.Preview || parsedFileType == FileType.Revision)
        {
            storagePath = _temporaryStoragePath;
        }
        else if (parsedFileType == FileType.Final)
        {
            storagePath = _permanentStoragePath;
        }
        else
        {
            storagePath = _fileStoragePath;
        }

        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
            {
                continue; // Skip empty files
            }

            ValidateFile(file, file.FileName);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var versionNumber = (parsedFileType == FileType.Preview && userRole == "Designer")
                ? currentVersion + 1
                : ++currentVersion;
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Use the storage path determined earlier based on file type
            var filePath = Path.Combine(storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var logoFile = new LogoFile
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                FileName = fileName,
                OriginalFileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileType = parsedFileType,
                VersionNumber = versionNumber,
                Description = description,
                IsVisibleToClient = isVisibleToClient,
                IsAdminApproved = isAdminApproved,
                IsFinalVersion = parsedFileType == FileType.Final,
                UploadedBy = uploadedBy,
                PreviewBatchId = previewBatchId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = uploadedBy
            };

            _context.LogoFiles.Add(logoFile);

            results.Add(new FileUploadResponseDto
            {
                Id = logoFile.Id,
                FileName = logoFile.FileName,
                OriginalFileName = logoFile.OriginalFileName,
                FileSize = logoFile.FileSize,
                ContentType = logoFile.ContentType,
                UploadedAt = logoFile.CreatedAt
            });
        }

        // Designer uploads do NOT change order status to PreviewDelivered.
        // Status changes to PreviewDelivered only when admin forwards/approves files via SendFilesToClientAsync.
        // This ensures client gets "Action Required" only when they can actually see and review the files.

        await _context.SaveChangesAsync();

        if (userRole == "Designer" && parsedFileType == FileType.Preview && results.Count > 0)
        {
            _logger.LogInformation("PreviewUploaded. OrderId={OrderId}, UserId={UserId}, FileCount={FileCount}", orderId, uploadedBy, results.Count);
        }

        // When designer uploads preview files, notify Admin/SuperAdmin (SignalR + toast/notification bell)
        if (userRole == "Designer" && parsedFileType == FileType.Preview && results.Count > 0)
        {
            try
            {
                var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                await _entityUpdateSender.SendPreviewUploadedAsync(orderId, adminUserIds);

                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Preview Ready for QA";
                var message = $"Designer uploaded preview files for order (#{orderNumber})";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send PreviewUploaded notification for order {OrderId}.", orderId);
            }
        }

        // When designer uploads final files, notify Admin/SuperAdmin
        if (userRole == "Designer" && parsedFileType == FileType.Final && results.Count > 0)
        {
            try
            {
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Final Files Uploaded";
                var message = $"Designer uploaded final files for order (#{orderNumber}). Review and approve designer price if needed.";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send FinalUploaded notification for order {OrderId}.", orderId);
            }
        }

        // When client uploads reference files, notify Admin and SuperAdmin
        if (userRole == "Client" && parsedFileType == FileType.Reference && results.Count > 0)
        {
            try
            {
                var orderWithClient = await _context.LogoOrders
                    .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
                if (orderWithClient?.Client != null)
                {
                    var clientName = $"{orderWithClient.Client.User.FirstName} {orderWithClient.Client.User.LastName}".Trim();
                    if (string.IsNullOrEmpty(clientName)) clientName = "Client";
                    var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                    var title = "File Uploaded by Client";
                    var message = $"{clientName} uploaded files for order (#{orderNumber})";
                    await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                    await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.FileUpload, NotificationReferenceType.Order, orderId, uploadedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create file upload notification for order {OrderId}.", orderId);
            }
        }

        return results;
    }

    public async Task<(byte[] fileContent, string fileName, string contentType)> DownloadFileAsync(
        Guid fileId, Guid? userId, string? userRole)
    {
        var file = await _context.LogoFiles
            .Include(f => f.Order)
                .ThenInclude(o => o.Client)
            .Include(f => f.Order)
                .ThenInclude(o => o.Designer)
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

        if (file == null)
        {
            throw new FileNotFoundException("File not found.");
        }

        if (file.Order == null)
            throw new InvalidOperationException("File has no associated order.");

        // Authorization
        // Use ForbiddenAccessException for authorization failures (user is authenticated but lacks permission)
        
        // Admin and SuperAdmin have access to all files
        if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            // Allow access - no additional checks needed
        }
        else if (userRole == "Client")
        {
            if (file.Order.Client == null)
                throw new InvalidOperationException("Order has no associated client.");
            // Clients can only access files from their own orders
            if (file.Order.Client.UserId != userId)
            {
                throw new ForbiddenAccessException("You don't have access to this file.");
            }
            // If we reach here, the client owns the order - allow access
        }
        else if (userRole == "Designer")
        {
            // For designers, we need to check if the order is assigned to their DesignerProfile
            // order.DesignerId is the DesignerProfile.Id, not the User.Id
            if (file.Order?.DesignerId == null)
            {
                throw new ForbiddenAccessException("You don't have access to this file.");
            }

            // Get the designer's profile to compare DesignerProfile.Id
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || file.Order.DesignerId != designer.Id)
            {
                throw new ForbiddenAccessException("You don't have access to this file.");
            }
            // If we reach here, the designer is assigned to the order - allow access
        }
        else
        {
            // Unknown role or null role - deny access
            throw new ForbiddenAccessException("You don't have access to this file.");
        }

        if (!System.IO.File.Exists(file.FilePath))
        {
            throw new FileNotFoundException("Physical file not found.");
        }

        var fileContent = await System.IO.File.ReadAllBytesAsync(file.FilePath);

        return (fileContent, file.OriginalFileName, file.ContentType);
    }

    public async Task<List<FileResponseDto>> GetOrderFilesAsync(Guid orderId, Guid? userId, string? userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        // Authorization
        // Use ForbiddenAccessException for authorization failures (user is authenticated but lacks permission)
        if (userRole == "Client" && order.Client.UserId != userId)
        {
            throw new ForbiddenAccessException("You don't have access to this order's files.");
        }

        if (userRole == "Designer")
        {
            // For designers, we need to check if the order is assigned to their DesignerProfile
            // order.DesignerId is the DesignerProfile.Id, not the User.Id
            if (order.DesignerId == null)
            {
                throw new ForbiddenAccessException("You don't have access to this order's files.");
            }

            // Get the designer's profile to compare DesignerProfile.Id
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
            {
                throw new ForbiddenAccessException("You don't have access to this order's files.");
            }
        }

        var filesQuery = _context.LogoFiles
            .Include(f => f.Order)
            .Where(f => f.OrderId == orderId && !f.IsDeleted);

        // Filter files based on role - clients only see approved/visible files
        if (userRole == "Client")
        {
            filesQuery = filesQuery.Where(f => f.IsVisibleToClient);
        }

        var files = await filesQuery
            .Include(f => f.ApprovedByUser)
            .ToListAsync();

        var result = _mapper.Map<List<FileResponseDto>>(files);
        
        // Batch-load user names to avoid N+1 queries
        var userIds = files.Select(f => f.UploadedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var approvedByIds = files.Where(f => f.ApprovedBy.HasValue).Select(f => f.ApprovedBy!.Value).Distinct().ToList();
        var allUserIds = userIds.Union(approvedByIds).Distinct().ToList();
        var users = allUserIds.Count > 0
            ? await _context.Users.Where(u => allUserIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            if (users.TryGetValue(file.UploadedBy, out var uploadedByName))
                fileDto.UploadedByName = uploadedByName;
            if (file.ApprovedByUser != null)
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
        }

        // Hide uploaded by information from designers
        if (userRole == "Designer")
        {
            foreach (var fileDto in result)
            {
                fileDto.UploadedBy = Guid.Empty;
                fileDto.UploadedByName = string.Empty;
            }
        }

        // Mask designer identity from clients (mediated workflow: client must never see designer)
        if (userRole == "Client" && userId.HasValue)
        {
            foreach (var fileDto in result)
            {
                var file = files.First(f => f.Id == fileDto.Id);
                if (file.FileType == FileType.Preview || file.UploadedBy != userId.Value)
                {
                    fileDto.UploadedBy = Guid.Empty;
                    fileDto.UploadedByName = "Company Design Team";
                }
            }
        }

        return result;
    }

    public async Task<List<FileResponseDto>> GetOrderFilesForOrdersAsync(List<Guid> orderIds, Guid userId, string userRole)
    {
        if (orderIds == null || orderIds.Count == 0)
            return new List<FileResponseDto>();

        var filesQuery = _context.LogoFiles
            .Include(f => f.Order)
                .ThenInclude(o => o.Client)
            .Include(f => f.Order)
                .ThenInclude(o => o.Designer)
            .Include(f => f.ApprovedByUser)
            .Where(f => orderIds.Contains(f.OrderId) && !f.IsDeleted);

        if (userRole == "Client")
        {
            filesQuery = filesQuery.Where(f => f.Order.Client != null && f.Order.Client.UserId == userId && f.IsVisibleToClient);
        }
        else if (userRole == "Designer")
        {
            var designer = await _context.DesignerProfiles.FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
            if (designer == null)
                return new List<FileResponseDto>();
            filesQuery = filesQuery.Where(f => f.Order.DesignerId == designer.Id);
        }

        var files = await filesQuery.ToListAsync();
        var result = _mapper.Map<List<FileResponseDto>>(files);

        var userIds = files.Select(f => f.UploadedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var approvedByIds = files.Where(f => f.ApprovedBy.HasValue).Select(f => f.ApprovedBy!.Value).Distinct().ToList();
        var allUserIds = userIds.Union(approvedByIds).Distinct().ToList();
        var users = allUserIds.Count > 0
            ? await _context.Users.Where(u => allUserIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            if (users.TryGetValue(file.UploadedBy, out var uploadedByName))
                fileDto.UploadedByName = uploadedByName;
            if (file.ApprovedByUser != null)
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
        }

        if (userRole == "Designer")
        {
            foreach (var fileDto in result)
            {
                fileDto.UploadedBy = Guid.Empty;
                fileDto.UploadedByName = string.Empty;
            }
        }

        if (userRole == "Client")
        {
            foreach (var fileDto in result)
            {
                var file = files.First(f => f.Id == fileDto.Id);
                if (file.FileType == FileType.Preview || file.UploadedBy != userId)
                {
                    fileDto.UploadedBy = Guid.Empty;
                    fileDto.UploadedByName = "Company Design Team";
                }
            }
        }

        return result;
    }

    public async Task<List<FileResponseDto>> GetOrderFilesForAdminAsync(Guid orderId)
    {
        var files = await _context.LogoFiles
            .Include(f => f.ApprovedByUser)
            .Where(f => f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();

        var result = _mapper.Map<List<FileResponseDto>>(files);
        
        var userIds = files.Select(f => f.UploadedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var users = userIds.Count > 0
            ? await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            if (users.TryGetValue(file.UploadedBy, out var uploadedByName))
                fileDto.UploadedByName = uploadedByName;
            if (file.ApprovedByUser != null)
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
        }

        return result;
    }

    public async Task<List<FileResponseDto>> GetAllFilesAsync(Guid? userId, string? userRole)
    {
        IQueryable<LogoFile> filesQuery = _context.LogoFiles
            .Include(f => f.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .Include(f => f.Order)
                .ThenInclude(o => o.Designer)
            .Include(f => f.ApprovedByUser)
            .Where(f => !f.IsDeleted && f.Order != null && !f.Order.IsDeleted);

        // Filter files based on role
        if (userRole == "Client")
        {
            // Clients can only see files from their own orders that are visible
            // Include Final files (which are always visible after approval) or files explicitly marked as visible
            filesQuery = filesQuery
                .Where(f => f.Order.Client != null && 
                           f.Order.Client.User != null &&
                           f.Order.Client.UserId == userId && 
                           (f.IsVisibleToClient || f.FileType == FileType.Final));
        }
        else if (userRole == "Designer")
        {
            // Designers can see files from orders assigned to them
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer != null)
            {
                filesQuery = filesQuery.Where(f => f.Order.DesignerId == designer.Id);
            }
            else
            {
                // No designer profile found, return empty list
                return new List<FileResponseDto>();
            }
        }
        // Admin and SuperAdmin can see all files (no additional filtering)

        var files = await filesQuery.ToListAsync();

        var result = _mapper.Map<List<FileResponseDto>>(files);
        
        // Batch-load user names to avoid N+1 queries
        var userIds = files.Select(f => f.UploadedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var approvedByIds = files.Where(f => f.ApprovedBy.HasValue).Select(f => f.ApprovedBy!.Value).Distinct().ToList();
        var allUserIds = userIds.Union(approvedByIds).Distinct().ToList();
        var users = allUserIds.Count > 0
            ? await _context.Users.Where(u => allUserIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            if (users.TryGetValue(file.UploadedBy, out var uploadedByName))
                fileDto.UploadedByName = uploadedByName;
            if (file.ApprovedByUser != null)
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
        }

        // Hide uploaded by information from designers
        if (userRole == "Designer")
        {
            foreach (var fileDto in result)
            {
                fileDto.UploadedBy = Guid.Empty;
                fileDto.UploadedByName = string.Empty;
            }
        }

        // Mask designer identity from clients (mediated workflow: client must never see designer)
        if (userRole == "Client" && userId.HasValue)
        {
            foreach (var fileDto in result)
            {
                var file = files.First(f => f.Id == fileDto.Id);
                if (file.FileType == FileType.Preview || file.UploadedBy != userId.Value)
                {
                    fileDto.UploadedBy = Guid.Empty;
                    fileDto.UploadedByName = "Company Design Team";
                }
            }
        }

        return result;
    }

    public async Task<PagedResultDto<FileResponseDto>> GetAllFilesPagedAsync(Guid? userId, string? userRole, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<LogoFile> filesQuery = _context.LogoFiles
            .Include(f => f.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .Include(f => f.Order)
                .ThenInclude(o => o.Designer)
            .Include(f => f.ApprovedByUser)
            .Where(f => !f.IsDeleted && f.Order != null && !f.Order.IsDeleted);

        if (userRole == "Client")
        {
            filesQuery = filesQuery
                .Where(f => f.Order.Client != null && 
                           f.Order.Client.User != null &&
                           f.Order.Client.UserId == userId && 
                           (f.IsVisibleToClient || f.FileType == FileType.Final));
        }
        else if (userRole == "Designer")
        {
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
            if (designer == null)
                return new PagedResultDto<FileResponseDto> { Items = new List<FileResponseDto>(), Total = 0, Page = page, PageSize = pageSize };
            filesQuery = filesQuery.Where(f => f.Order.DesignerId == designer.Id);
        }

        var total = await filesQuery.CountAsync();
        var files = await filesQuery
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = _mapper.Map<List<FileResponseDto>>(files);
        var userIds = files.Select(f => f.UploadedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var approvedByIds = files.Where(f => f.ApprovedBy.HasValue).Select(f => f.ApprovedBy!.Value).Distinct().ToList();
        var allUserIds = userIds.Union(approvedByIds).Distinct().ToList();
        var users = allUserIds.Count > 0
            ? await _context.Users.Where(u => allUserIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            if (users.TryGetValue(file.UploadedBy, out var uploadedByName))
                fileDto.UploadedByName = uploadedByName;
            if (file.ApprovedByUser != null)
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
        }

        if (userRole == "Designer")
        {
            foreach (var fileDto in result)
            {
                fileDto.UploadedBy = Guid.Empty;
                fileDto.UploadedByName = string.Empty;
            }
        }

        if (userRole == "Client" && userId.HasValue)
        {
            foreach (var fileDto in result)
            {
                var file = files.First(f => f.Id == fileDto.Id);
                if (file.FileType == FileType.Preview || file.UploadedBy != userId.Value)
                {
                    fileDto.UploadedBy = Guid.Empty;
                    fileDto.UploadedByName = "Company Design Team";
                }
            }
        }

        return new PagedResultDto<FileResponseDto>
        {
            Items = result,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FileResponseDto> ApproveFileAsync(Guid fileId, ApproveFileDto request, Guid approvedBy)
    {
        var file = await _context.LogoFiles
            .Include(f => f.Order)
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

        if (file == null)
        {
            throw new InvalidOperationException("File not found.");
        }

        if (OrderLockingHelper.IsOrderLocked(file.Order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        file.IsAdminApproved = request.Approved;
        file.IsVisibleToClient = request.MakeVisibleToClient && request.Approved;
        file.ApprovedBy = approvedBy;
        file.ApprovedAt = DateTime.UtcNow;
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedBy = approvedBy;

        await _context.SaveChangesAsync();

        var result = _mapper.Map<FileResponseDto>(file);
        var uploadedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == file.UploadedBy);
        var approvedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == approvedBy);
        
        if (uploadedByUser != null)
        {
            result.UploadedByName = $"{uploadedByUser.FirstName} {uploadedByUser.LastName}";
        }
        if (approvedByUser != null)
        {
            result.ApprovedByName = $"{approvedByUser.FirstName} {approvedByUser.LastName}";
        }

        return result;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, Guid userId, string userRole)
    {
        var file = await _context.LogoFiles
            .Include(f => f.Order)
                .ThenInclude(o => o.Client)
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

        if (file == null)
        {
            throw new FileNotFoundException("File not found.");
        }

        if (OrderLockingHelper.IsOrderLocked(file.Order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        // Only client or SuperAdmin can delete
        if (userRole != "SuperAdmin" && (userRole != "Client" || file.Order?.Client == null || file.Order.Client.UserId != userId))
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this file.");
        }

        // Soft delete
        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        file.DeletedBy = userId;

        // Optionally delete physical file
        if (System.IO.File.Exists(file.FilePath))
        {
            System.IO.File.Delete(file.FilePath);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task VerifyOrderUploadAccessAsync(LogoOrder order, Guid uploadedBy)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == uploadedBy);
        var userRole = user?.Role?.Name ?? string.Empty;

        if (userRole == "Client")
        {
            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == uploadedBy && !c.IsDeleted);
            if (client == null || order.ClientId != client.Id)
                throw new ForbiddenAccessException("You don't have access to upload files to this order.");
        }
        else if (userRole == "Designer")
        {
            var designer = await _context.DesignerProfiles.FirstOrDefaultAsync(d => d.UserId == uploadedBy && !d.IsDeleted);
            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to upload files to this order.");
        }
        // Admin/SuperAdmin: allow
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
}
