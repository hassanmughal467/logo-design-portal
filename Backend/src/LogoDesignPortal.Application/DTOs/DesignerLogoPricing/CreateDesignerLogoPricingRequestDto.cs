using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.DesignerLogoPricing;

public class CreateDesignerLogoPricingRequestDto
{
    [Required]
    public Guid DesignerId { get; set; }

    [Required]
    public DesignCategory DesignCategory { get; set; }

    [Required]
    public DesignType DesignType { get; set; }

    /// <summary>Default payout price in PKR. 0 = Custom (designer must enter proposed price).</summary>
    [Range(0, double.MaxValue)]
    public decimal DefaultPrice { get; set; }

    public bool IsActive { get; set; } = true;
}
