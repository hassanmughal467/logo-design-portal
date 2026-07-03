namespace LogoDesignPortal.Application.Helpers;

/// <summary>Normalizes ISO 4217 currency codes for client profiles, quotes, and orders.</summary>
public static class ClientCurrencyHelper
{
    public const string DefaultCode = "USD";

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "GBP", "EUR", "PKR", "CAD", "AUD", "INR", "JPY"
    };

    public static string NormalizeOrDefault(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return DefaultCode;
        }

        var normalized = code.Trim().ToUpperInvariant();
        return Allowed.Contains(normalized) ? normalized : DefaultCode;
    }

    public static bool IsAllowed(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Allowed.Contains(code.Trim());

    /// <summary>
    /// Picks a single ISO code when all sources agree; otherwise marks mixed (caller may label KPIs "Mixed").
    /// </summary>
    public static (string Code, bool Mixed) ResolveAggregateCurrency(IEnumerable<string?> codes)
    {
        var distinct = codes
            .Select(NormalizeOrDefault)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (distinct.Count == 1)
        {
            return (distinct[0], false);
        }

        if (distinct.Count > 1)
        {
            return (DefaultCode, true);
        }

        return (DefaultCode, false);
    }

    /// <summary>
    /// Currency for a client's revenue KPIs/charts. Completed orders are authoritative when they
    /// share one ISO code; otherwise falls back to <see cref="ClientProfile.CurrencyCode"/>.
    /// </summary>
    public static string ResolveClientRevenueCurrency(string? profileCode, IEnumerable<string?>? orderCurrencyCodes)
    {
        var orderCodes = (orderCurrencyCodes ?? Enumerable.Empty<string?>())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (orderCodes.Count == 1)
        {
            return NormalizeOrDefault(orderCodes[0]);
        }

        if (orderCodes.Count > 1)
        {
            return ResolveAggregateCurrency(orderCodes).Code;
        }

        return NormalizeOrDefault(profileCode);
    }
}
