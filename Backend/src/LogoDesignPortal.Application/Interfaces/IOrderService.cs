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
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, Guid userId);
}
