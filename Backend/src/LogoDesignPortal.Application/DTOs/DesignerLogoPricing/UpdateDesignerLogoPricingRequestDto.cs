using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.DesignerLogoPricing;

public class UpdateDesignerLogoPricingRequestDto
{
    /// <summary>Default payout price in PKR. 0 = Custom (designer must enter proposed price).</summary>
    [Range(0, double.MaxValue)]
    public decimal DefaultPrice { get; set; }

    public bool IsActive { get; set; }
}
