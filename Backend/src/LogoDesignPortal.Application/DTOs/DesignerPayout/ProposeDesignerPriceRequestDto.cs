using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Request for designer to propose a complexity price. Used when designer reviews logo and requests higher payout.
/// </summary>
public class ProposeDesignerPriceRequestDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Proposed price must be greater than zero.")]
    public decimal ProposedPrice { get; set; }

    /// <summary>Optional note to admin (shown in designer payout negotiation history).</summary>
    [MaxLength(2000)]
    public string? Message { get; set; }
}
