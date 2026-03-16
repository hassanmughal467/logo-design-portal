using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Default pricing for design types (PKR).
/// </summary>
public static class DesignPricingHelper
{
    /// <summary>Default price for LeftChest embroidery (PKR).</summary>
    public const decimal LeftChestDefaultPrice = 350m;

    /// <summary>Default price for JacketBack embroidery (PKR).</summary>
    public const decimal JacketBackDefaultPrice = 700m;

    /// <summary>Default price for SimpleVector (PKR).</summary>
    public const decimal SimpleVectorDefaultPrice = 350m;

    /// <summary>ComplexVector has no default; custom price required.</summary>
    public static readonly decimal? ComplexVectorDefaultPrice = null;

    /// <summary>
    /// Gets the default price for the given design type, or null if custom price is required.
    /// </summary>
    public static decimal? GetDefaultPrice(DesignType designType)
    {
        return designType switch
        {
            DesignType.LeftChest => LeftChestDefaultPrice,
            DesignType.JacketBack => JacketBackDefaultPrice,
            DesignType.SimpleVector => SimpleVectorDefaultPrice,
            DesignType.ComplexVector => ComplexVectorDefaultPrice,
            _ => null
        };
    }

    /// <summary>
    /// Returns true if the design type has a standard default price (designer can use it or propose different).
    /// </summary>
    public static bool HasDefaultPrice(DesignType designType)
    {
        return GetDefaultPrice(designType).HasValue;
    }

    /// <summary>
    /// Returns true if the proposed price differs from the standard default (requires admin approval).
    /// For ComplexVector, any proposed price is considered "differs" since there is no default.
    /// </summary>
    public static bool PriceDiffersFromStandard(DesignType designType, decimal proposedPrice)
    {
        var defaultPrice = GetDefaultPrice(designType);
        if (!defaultPrice.HasValue)
            return true; // ComplexVector always requires approval
        return proposedPrice != defaultPrice.Value;
    }
}
