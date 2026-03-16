using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Client-specific logo pricing with multi-currency support.
/// Used when client creates order - lookup by ClientId, DesignCategory, DesignType.
/// Designer payout continues to use DesignPricing.
/// </summary>
public class ClientLogoPricing : BaseEntity
{
    public Guid ClientId { get; set; }
    public DesignCategory DesignCategory { get; set; }
    public DesignType DesignType { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ClientProfile Client { get; set; } = null!;
}
