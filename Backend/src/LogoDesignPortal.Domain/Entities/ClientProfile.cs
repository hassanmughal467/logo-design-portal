namespace LogoDesignPortal.Domain.Entities;

public class ClientProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Cell { get; set; }
    public string? Fax { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Website { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; } // Internal admin notes

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<LogoOrder> Orders { get; set; } = new List<LogoOrder>();
}
