using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Users;

public class CreateDesignerProfileRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string? Specialization { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public decimal? HourlyRate { get; set; }

    public bool IsAvailable { get; set; } = true;
}
