using LogoDesignPortal.Application.DTOs.Orders;

namespace LogoDesignPortal.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, Guid clientId);
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId, Guid? userId, string? userRole);
    Task<List<OrderResponseDto>> GetOrdersByClientAsync(Guid clientId);
    Task<List<OrderResponseDto>> GetOrdersByDesignerAsync(Guid designerId);
    Task<List<OrderResponseDto>> GetAllOrdersAsync(string? userRole);
    Task<OrderResponseDto> AssignOrderToDesignerAsync(Guid orderId, Guid designerId, Guid assignedBy);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, Guid userId, string? userRole = null);
    Task<OrderResponseDto> RequestPriceApprovalAsync(Guid orderId, RequestPriceApprovalDto request, Guid requestedBy);
    Task<OrderResponseDto> ApprovePriceAsync(Guid orderId, ApprovePriceDto request, Guid approvedBy);
    Task<OrderResponseDto> ApproveOrderAsync(Guid orderId, Guid approvedBy);
    Task<OrderResponseDto> SendFilesToClientAsync(Guid orderId, List<Guid> fileIds, Guid sentBy);
    Task<OrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderRequestDto request, Guid userId);
    Task<OrderResponseDto> CancelOrderAsync(Guid orderId, CancelOrderRequestDto request, Guid userId, string userRole);
    Task<OrderResponseDto> ArchiveOrderAsync(Guid orderId, ArchiveOrderRequestDto? request, Guid userId);
    Task<OrderResponseDto> UnarchiveOrderAsync(Guid orderId, Guid userId);
    Task<OrderResponseDto> RefundOrderAsync(Guid orderId, RefundOrderRequestDto request, Guid userId);
    Task<List<OrderLogResponseDto>> GetOrderLogsAsync(Guid orderId);
    Task<OrderResponseDto> SetAllowUploadsAsync(Guid orderId, bool allowUploads, Guid userId);
}
