using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Quotes;

public class RespondQuoteRequestDto
{
    [Required]
    [Range(0.01, 1000000)]
    public decimal AdminQuotedPrice { get; set; }

    [MaxLength(2000)]
    public string? AdminNotes { get; set; }
}
