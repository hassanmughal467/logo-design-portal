using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;

namespace LogoDesignPortal.Application.Interfaces;

public interface IRevisionService
{
    Task<RevisionResponseDto> RequestRevisionAsync(Guid orderId, RequestRevisionDto request, Guid requestedBy);
    Task<RevisionResponseDto?> GetLatestRevisionAsync(Guid orderId, Guid? userId, string? userRole);
    Task<(byte[] Content, string FileName, string ContentType)> DownloadRevisionFileAsync(Guid fileId, Guid userId, string? userRole);
    Task<OrderResponseDto> ApproveLogoAsync(Guid orderId, ApproveLogoDto request, Guid approvedBy);
    Task<bool> CanRequestRevisionAsync(Guid orderId, Guid userId, string userRole);
    Task<bool> CanApproveLogoAsync(Guid orderId, Guid userId, string userRole);
}
