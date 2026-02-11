namespace LogoDesignPortal.Domain.Entities;

public class OrderComment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public bool IsInternal { get; set; } = true; // Internal comments (Admin/Designer only), not visible to clients

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
