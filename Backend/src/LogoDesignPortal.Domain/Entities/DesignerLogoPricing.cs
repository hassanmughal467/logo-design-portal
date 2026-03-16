using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Designer-specific default pricing for design categories and types (PKR).
/// Used when designer submits pricing - lookup by DesignerId, DesignCategory, DesignType.
/// Falls back to global DesignPricing when no designer-specific pricing exists.
/// </summary>
public class DesignerLogoPricing : BaseEntity
{
    public Guid DesignerId { get; set; }
    public DesignCategory DesignCategory { get; set; }
    public DesignType DesignType { get; set; }
    /// <summary>Default payout price in PKR. 0 = Custom (designer must enter).</summary>
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public DesignerProfile Designer { get; set; } = null!;
}
