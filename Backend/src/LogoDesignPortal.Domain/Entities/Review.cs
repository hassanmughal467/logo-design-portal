namespace LogoDesignPortal.Domain.Entities;

public class Review : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ClientId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsPublished { get; set; } = true;

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
    public ClientProfile Client { get; set; } = null!;
}
