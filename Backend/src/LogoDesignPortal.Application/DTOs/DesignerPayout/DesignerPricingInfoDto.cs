namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Default pricing info for design types (for UI display).
/// </summary>
public class DesignerPricingInfoDto
{
    public string DesignCategory { get; set; } = string.Empty;
    public string DesignType { get; set; } = string.Empty;
    public decimal? DefaultPrice { get; set; }
    public bool RequiresCustomPrice { get; set; }
}
