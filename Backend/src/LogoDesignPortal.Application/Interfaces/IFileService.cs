using LogoDesignPortal.Application.DTOs.Files;
using Microsoft.AspNetCore.Http;

namespace LogoDesignPortal.Application.Interfaces;

public interface IFileService
{
    Task<FileUploadResponseDto> UploadFileAsync(Guid orderId, IFormFile file, Guid uploadedBy);
    Task<(byte[] fileContent, string fileName, string contentType)> DownloadFileAsync(Guid fileId, Guid? userId, string? userRole);
    Task<List<FileUploadResponseDto>> GetOrderFilesAsync(Guid orderId, Guid? userId, string? userRole);
    Task<bool> DeleteFileAsync(Guid fileId, Guid userId, string userRole);
}
