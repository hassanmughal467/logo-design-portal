using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Request for designer to submit design category, type, and proposed price when uploading final files.
/// </summary>
public class SubmitDesignerPricingRequestDto
{
    public DesignCategory DesignCategory { get; set; }
    public DesignType DesignType { get; set; }
    /// <summary>Standard/default price from DesignPricing table. Optional; server computes if not provided.</summary>
    public decimal? StandardPrice { get; set; }
    public decimal ProposedPrice { get; set; }
    /// <summary>Optional reason when proposed price differs from standard (for admin review).</summary>
    public string? Reason { get; set; }
}
