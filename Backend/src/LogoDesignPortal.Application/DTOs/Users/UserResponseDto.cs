namespace LogoDesignPortal.Application.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsRootAdmin { get; set; }
    public DateTime CreatedAt { get; set; }

    // Additional User fields
    public string? SecondaryEmail { get; set; }
    public string? InvoiceEmail { get; set; }

    // Profile information (null if profile doesn't exist)
    public ClientProfileDto? ClientProfile { get; set; }
    public DesignerProfileDto? DesignerProfile { get; set; }
}
