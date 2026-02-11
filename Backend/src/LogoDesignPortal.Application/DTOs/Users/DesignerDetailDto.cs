using LogoDesignPortal.Application.DTOs.Orders;

namespace LogoDesignPortal.Application.DTOs.Users;

public class DesignerDetailDto
{
    public UserResponseDto User { get; set; } = null!;
    public DesignerProfileDto? DesignerProfile { get; set; }
    public List<OrderResponseDto> AssignedOrders { get; set; } = new();
    public int CompletedOrdersCount { get; set; }
    public double? AverageDeliveryTimeDays { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}
