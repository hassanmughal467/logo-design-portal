namespace LogoDesignPortal.Domain.Entities;

public class ClientProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<LogoOrder> Orders { get; set; } = new List<LogoOrder>();
}
