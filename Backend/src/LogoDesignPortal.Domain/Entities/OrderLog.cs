using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class OrderLog : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderAction Action { get; set; }
    public OrderStatus? PreviousStatus { get; set; }
    public OrderStatus? NewStatus { get; set; }
    public string? PerformedBy { get; set; } // User role or "System"
    public Guid? PerformedById { get; set; } // User ID if performed by user
    public string? Note { get; set; }
    public string? Metadata { get; set; } // JSON string for additional data

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
}
