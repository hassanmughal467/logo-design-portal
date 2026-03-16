using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.ClientLogoPricing;

public class CreateClientLogoPricingRequestDto
{
    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public DesignCategory DesignCategory { get; set; }

    [Required]
    public DesignType DesignType { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = "USD";

    public bool IsActive { get; set; } = true;
}
