namespace LogoDesignPortal.Application.DTOs.Users;

public class DesignerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}
