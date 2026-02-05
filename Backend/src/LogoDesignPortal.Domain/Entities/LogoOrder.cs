using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class LogoOrder : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid? DesignerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Price { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Requirements { get; set; }
    public string? ColorPreferences { get; set; }
    public string? StylePreferences { get; set; }

    // Navigation properties
    public ClientProfile Client { get; set; } = null!;
    public DesignerProfile? Designer { get; set; }
    public ICollection<LogoFile> Files { get; set; } = new List<LogoFile>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public Invoice? Invoice { get; set; }
}
