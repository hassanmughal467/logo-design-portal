namespace LogoDesignPortal.Domain.Entities;

public class DesignerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<LogoOrder> AssignedOrders { get; set; } = new List<LogoOrder>();
}
