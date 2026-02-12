namespace LogoDesignPortal.Application.DTOs.Users;

public class DesignerInfoDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public bool IsAvailable { get; set; }
}
