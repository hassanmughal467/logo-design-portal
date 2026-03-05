namespace LogoDesignPortal.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    public Guid? DeactivatedBy { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiryTime { get; set; }
    
    // Account lockout fields
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    
    // Additional user information fields
    public string? SecondaryEmail { get; set; }
    public string? InvoiceEmail { get; set; }

    /// <summary>Root Admin cannot be deleted, deactivated, or have their role changed.</summary>
    public bool IsRootAdmin { get; set; } = false;

    // Navigation properties
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ClientProfile? ClientProfile { get; set; }
    public DesignerProfile? DesignerProfile { get; set; }
}
