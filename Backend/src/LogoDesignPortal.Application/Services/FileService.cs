using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.Application.Services;

public class FileService : IFileService
{
    private readonly IApplicationDbContext _context;
    private readonly string _fileStoragePath;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public FileService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _fileStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        
        if (!Directory.Exists(_fileStoragePath))
        {
            Directory.CreateDirectory(_fileStoragePath);
        }
    }

    public async Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
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

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_fileStoragePath, fileName);

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
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };

        _context.LogoFiles.Add(logoFile);
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
        if (userRole == "Client" && file.Order.Client.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this file.");
        }

        if (userRole == "Designer" && file.Order.DesignerId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this file.");
        }

        if (!System.IO.File.Exists(file.FilePath))
        {
            throw new FileNotFoundException("Physical file not found.");
        }

        var fileContent = await System.IO.File.ReadAllBytesAsync(file.FilePath);

        return (fileContent, file.OriginalFileName, file.ContentType);
    }

    public async Task<List<FileUploadResponseDto>> GetOrderFilesAsync(Guid orderId, Guid? userId, string? userRole)
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
        if (userRole == "Client" && order.Client.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this order's files.");
        }

        if (userRole == "Designer" && order.DesignerId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this order's files.");
        }

        var files = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();

        return files.Select(f => new FileUploadResponseDto
        {
            Id = f.Id,
            FileName = f.FileName,
            OriginalFileName = f.OriginalFileName,
            FileSize = f.FileSize,
            ContentType = f.ContentType,
            UploadedAt = f.CreatedAt
        }).ToList();
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
