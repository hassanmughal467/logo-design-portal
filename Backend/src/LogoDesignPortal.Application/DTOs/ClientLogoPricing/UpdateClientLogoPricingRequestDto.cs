using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.ClientLogoPricing;

public class UpdateClientLogoPricingRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = "USD";

    public bool IsActive { get; set; }
}
