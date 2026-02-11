using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Revisions;

public class CreateRevisionRequestDto
{
    [Required]
    [MinLength(10)]
    public string Instructions { get; set; } = string.Empty;
}
