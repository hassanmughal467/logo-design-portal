namespace LogoDesignPortal.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ClientProfile? ClientProfile { get; set; }
    public DesignerProfile? DesignerProfile { get; set; }
}
