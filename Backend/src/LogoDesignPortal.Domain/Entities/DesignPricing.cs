using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Configurable default pricing for design categories and types.
/// Used when designers submit pricing to display default amounts.
/// </summary>
public class DesignPricing : BaseEntity
{
    public DesignCategory DesignCategory { get; set; }
    public DesignType DesignType { get; set; }
    /// <summary>Default price in PKR. 0 = Custom (designer must enter).</summary>
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
