namespace LogoDesignPortal.Application.DTOs.Users;

public class ClientInfoDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    // Address fields may be masked for Admin/Designer
}
