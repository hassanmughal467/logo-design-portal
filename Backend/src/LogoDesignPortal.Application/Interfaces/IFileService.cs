using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LogoDesignPortal.Application.Interfaces;

public interface IFileService
{
    /// <summary>
    /// Prepares reference files for order creation (writes to disk, returns entities).
    /// Used for atomic order+files creation. Does NOT add to context or save.
    /// </summary>
    Task<List<LogoFile>> PrepareReferenceFilesForOrderAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string? description = null);

    Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy, string fileType, string? description = null);
    Task<List<FileUploadResponseDto>> UploadMultipleFilesAsync(Guid orderId, IFormFile[] files, Guid uploadedBy, string fileType, string? description = null);
    Task<(byte[] fileContent, string fileName, string contentType)> DownloadFileAsync(Guid fileId, Guid? userId, string? userRole);
    Task<List<FileResponseDto>> GetOrderFilesAsync(Guid orderId, Guid? userId, string? userRole);
    Task<List<FileResponseDto>> GetAllFilesAsync(Guid? userId, string? userRole);
    Task<bool> DeleteFileAsync(Guid fileId, Guid userId, string userRole);
    Task<FileResponseDto> ApproveFileAsync(Guid fileId, ApproveFileDto request, Guid approvedBy);
    Task<List<FileResponseDto>> GetOrderFilesForAdminAsync(Guid orderId); // All files including unapproved
}
