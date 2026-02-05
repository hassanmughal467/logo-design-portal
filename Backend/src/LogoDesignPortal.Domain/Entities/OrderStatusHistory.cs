using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public Guid ChangedBy { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
}
