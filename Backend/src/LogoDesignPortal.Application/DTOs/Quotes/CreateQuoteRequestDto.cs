using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Quotes;

public class CreateQuoteRequestDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string LogoName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal? RequestedBudget { get; set; }
}
