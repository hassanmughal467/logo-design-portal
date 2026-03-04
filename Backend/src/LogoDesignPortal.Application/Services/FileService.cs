using AutoMapper;
using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.Exceptions;
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
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public FileService(IApplicationDbContext context, IConfiguration configuration, IMapper mapper, ILogger<FileService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
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

    public async Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy, string fileType, string? description = null)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        // Check if uploads are allowed for this order
        if (!order.AllowUploads)
        {
            throw new InvalidOperationException("File uploads are disabled for this order. Please contact an administrator to enable uploads.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("File size exceeds maximum allowed size (10MB).");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".ai", ".eps", ".psd" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("File type not allowed.");
        }

        // Parse file type enum first to determine storage location
        if (!Enum.TryParse<FileType>(fileType, true, out var parsedFileType))
        {
            parsedFileType = FileType.Reference;
        }

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

        // Get version number (increment from existing files)
        var existingFiles = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();
        var versionNumber = existingFiles.Any() ? existingFiles.Max(f => f.VersionNumber) + 1 : 1;

        // Determine visibility based on file type and uploader role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == uploadedBy);
        
        var userRole = user?.Role?.Name ?? string.Empty;
        
        // Fixed: Designer uploads are NEVER visible to clients until admin approval
        // Only client uploads of Reference type are visible immediately
        var isVisibleToClient = userRole == "Client" && parsedFileType == FileType.Reference;
        var isAdminApproved = userRole == "Client" && parsedFileType == FileType.Reference;

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

    public async Task<List<FileUploadResponseDto>> UploadMultipleFilesAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string fileType, string? description = null)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
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

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".ai", ".eps", ".psd" };
        var results = new List<FileUploadResponseDto>();

        // Get existing files to determine version numbers
        var existingFiles = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();
        var currentVersion = existingFiles.Any() ? existingFiles.Max(f => f.VersionNumber) : 0;

        // Parse file type enum
        if (!Enum.TryParse<FileType>(fileType, true, out var parsedFileType))
        {
            parsedFileType = FileType.Reference;
        }

        // Get uploader role to determine visibility
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == uploadedBy);
        
        var userRole = user?.Role?.Name ?? string.Empty;
        
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

            if (file.Length > MaxFileSize)
            {
                throw new InvalidOperationException($"File '{file.FileName}' exceeds maximum allowed size (10MB).");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"File type '{fileExtension}' is not allowed for file '{file.FileName}'.");
            }

            currentVersion++;
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
                VersionNumber = currentVersion,
                Description = description,
                IsVisibleToClient = isVisibleToClient,
                IsAdminApproved = isAdminApproved,
                IsFinalVersion = parsedFileType == FileType.Final,
                UploadedBy = uploadedBy,
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
}
