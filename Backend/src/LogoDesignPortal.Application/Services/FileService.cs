using AutoMapper;
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

namespace LogoDesignPortal.Application.Services;

public class FileService : IFileService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<FileService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] VectorExtensions = { ".svg", ".pdf", ".ai", ".eps", ".psd" };
    private const long ImageMaxBytes = 10 * 1024 * 1024; // 10MB
    private const long VectorMaxBytes = 25 * 1024 * 1024; // 25MB

    public FileService(IApplicationDbContext context, IConfiguration configuration, IMapper mapper, ILogger<FileService> logger, INotificationService notificationService, IRealtimeEntityUpdateSender entityUpdateSender)
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
            _logger.LogWarning(ex, "Cannot create file storage directories at {Path}. File uploads will fail until write permissions are granted to the IIS App Pool identity (e.g. IIS AppPool\\HawkBE) for this path.", _fileStoragePath);
        }
    }

    private static long GetMaxSizeForExtension(string extension)
    {
        var ext = extension.ToLowerInvariant();
        if (ImageExtensions.Contains(ext)) return ImageMaxBytes;
        if (VectorExtensions.Contains(ext)) return VectorMaxBytes;
        return ImageMaxBytes; // default for unknown
    }

    private static void ValidateFile(IFormFile file, string? fileNameForError = null)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = ImageExtensions.Concat(VectorExtensions).Distinct().ToArray();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed{(fileNameForError != null ? $" for file '{fileNameForError}'" : ".")}");
        var maxSize = GetMaxSizeForExtension(ext);
        if (file.Length > maxSize)
            throw new InvalidOperationException($"File {(fileNameForError != null ? $"'{fileNameForError}'" : "")} exceeds maximum allowed size ({(ImageExtensions.Contains(ext) ? "10MB" : "25MB")}).");
    }

    public async Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy, string fileType, string? description = null)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

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

        // Parse file type enum first to determine storage location
        if (!Enum.TryParse<FileType>(fileType, true, out var parsedFileType))
        {
            parsedFileType = FileType.Reference;
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

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == uploadedBy);
        var userRole = user?.Role?.Name ?? string.Empty;

        // Version = delivery round for Designer Preview (batch-based), per-file sequence for others
        int versionNumber;
        if (parsedFileType == FileType.Preview && userRole == "Designer")
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

        // If designer uploads preview files, update order status to PreviewDelivered
        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            order.Status = OrderStatus.PreviewDelivered;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = uploadedBy;
        }

        await _context.SaveChangesAsync();

        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            _logger.LogInformation("PreviewUploaded. OrderId={OrderId}, UserId={UserId}, FileId={FileId}", orderId, uploadedBy, logoFile.Id);
        }

        // When designer uploads preview files, notify Admin panel (PreviewUploaded)
        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            try
            {
                var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                await _entityUpdateSender.SendPreviewUploadedAsync(orderId, adminUserIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send PreviewUploaded SignalR event for order {OrderId}.", orderId);
            }
        }

        // When client uploads reference files, notify Admin
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
        var currentVersion = 0;

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;

            ValidateFile(file, file.FileName);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            currentVersion++;
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
                VersionNumber = currentVersion,
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

    public async Task<List<FileUploadResponseDto>> UploadMultipleFilesAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string fileType, string? description = null)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

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

        // If designer uploads preview files, update order status to PreviewDelivered
        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            order.Status = OrderStatus.PreviewDelivered;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = uploadedBy;
        }

        await _context.SaveChangesAsync();

        if (userRole == "Designer" && parsedFileType == FileType.Preview && results.Count > 0)
        {
            _logger.LogInformation("PreviewUploaded. OrderId={OrderId}, UserId={UserId}, FileCount={FileCount}", orderId, uploadedBy, results.Count);
        }

        // When designer uploads preview files, notify Admin panel (PreviewUploaded)
        if (userRole == "Designer" && parsedFileType == FileType.Preview)
        {
            try
            {
                var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                await _entityUpdateSender.SendPreviewUploadedAsync(orderId, adminUserIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send PreviewUploaded SignalR event for order {OrderId}.", orderId);
            }
        }

        // When client uploads reference files, notify Admin
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

        // Authorization
        // Use ForbiddenAccessException for authorization failures (user is authenticated but lacks permission)
        
        // Admin and SuperAdmin have access to all files
        if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            // Allow access - no additional checks needed
        }
        else if (userRole == "Client")
        {
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
            if (file.Order.DesignerId == null)
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
        
        // Populate user names
        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            var uploadedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == file.UploadedBy);
            if (uploadedByUser != null)
            {
                fileDto.UploadedByName = $"{uploadedByUser.FirstName} {uploadedByUser.LastName}";
            }
            if (file.ApprovedByUser != null)
            {
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
            }
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

    public async Task<List<FileResponseDto>> GetOrderFilesForAdminAsync(Guid orderId)
    {
        var files = await _context.LogoFiles
            .Include(f => f.ApprovedByUser)
            .Where(f => f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();

        var result = _mapper.Map<List<FileResponseDto>>(files);
        
        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            var uploadedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == file.UploadedBy);
            if (uploadedByUser != null)
            {
                fileDto.UploadedByName = $"{uploadedByUser.FirstName} {uploadedByUser.LastName}";
            }
            if (file.ApprovedByUser != null)
            {
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
            }
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
        
        // Populate user names
        foreach (var fileDto in result)
        {
            var file = files.First(f => f.Id == fileDto.Id);
            var uploadedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == file.UploadedBy);
            if (uploadedByUser != null)
            {
                fileDto.UploadedByName = $"{uploadedByUser.FirstName} {uploadedByUser.LastName}";
            }
            if (file.ApprovedByUser != null)
            {
                fileDto.ApprovedByName = $"{file.ApprovedByUser.FirstName} {file.ApprovedByUser.LastName}";
            }
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
        if (userRole != "SuperAdmin" && (userRole != "Client" || file.Order.Client.UserId != userId))
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
