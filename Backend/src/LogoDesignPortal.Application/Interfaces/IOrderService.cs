using LogoDesignPortal.Application.DTOs.Common;
using LogoDesignPortal.Application.DTOs.Orders;
using Microsoft.AspNetCore.Http;

namespace LogoDesignPortal.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, Guid clientId);
    /// <summary>
    /// Creates order with reference files atomically. Rolls back if any step fails.
    /// </summary>
    Task<OrderResponseDto> CreateOrderWithFilesAsync(CreateOrderRequestDto request, IFormFile[] files, Guid clientId, string? description = null);
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId, Guid? userId, string? userRole);
    Task<List<OrderResponseDto>> GetOrdersByClientAsync(Guid clientId);
    Task<List<OrderResponseDto>> GetOrdersByDesignerAsync(Guid designerId);
    Task<List<OrderResponseDto>> GetAllOrdersAsync(string? userRole);
    Task<PagedResultDto<OrderResponseDto>> GetOrdersPagedAsync(string? userRole, int page, int pageSize);
    Task<OrderResponseDto> AssignOrderToDesignerAsync(Guid orderId, Guid designerId, Guid assignedBy);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, Guid userId, string? userRole = null);
    Task<OrderResponseDto> RequestPriceApprovalAsync(Guid orderId, RequestPriceApprovalDto request, Guid requestedBy);
    Task<OrderResponseDto> ApprovePriceAsync(Guid orderId, ApprovePriceDto request, Guid approvedBy);
    Task<OrderResponseDto> ApproveOrderAsync(Guid orderId, Guid approvedBy);
    Task<OrderResponseDto> SendFilesToClientAsync(Guid orderId, List<Guid> fileIds, Guid sentBy);
    /// <summary>
    /// Sends entire preview batch to client. Admin must send full batch - no partial delivery.
    /// </summary>
    Task<OrderResponseDto> SendPreviewBatchToClientAsync(Guid orderId, Guid previewBatchId, Guid sentBy);
    Task<OrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderRequestDto request, Guid userId);
    Task<OrderResponseDto> CancelOrderAsync(Guid orderId, CancelOrderRequestDto request, Guid userId, string userRole);
    Task<OrderResponseDto> ArchiveOrderAsync(Guid orderId, ArchiveOrderRequestDto? request, Guid userId);
    Task<OrderResponseDto> UnarchiveOrderAsync(Guid orderId, Guid userId);
    Task<OrderResponseDto> RefundOrderAsync(Guid orderId, RefundOrderRequestDto request, Guid userId);
    Task<List<OrderLogResponseDto>> GetOrderLogsAsync(Guid orderId);
    /// <summary>
    /// Gets order logs with access control. Throws ForbiddenAccessException if user lacks access.
    /// </summary>
    Task<List<OrderLogResponseDto>> GetOrderLogsWithAccessAsync(Guid orderId, Guid userId, string? userRole);
    Task<OrderResponseDto> SetAllowUploadsAsync(Guid orderId, bool allowUploads, Guid userId);

    /// <summary>
    /// Admin updates the client charge price for an order. Can be done anytime before invoice generation.
    /// </summary>
    Task<OrderResponseDto> UpdateClientChargePriceAsync(Guid orderId, UpdateClientChargePriceRequestDto request, Guid userId, string? userRole = null);
}
