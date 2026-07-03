namespace LogoDesignPortal.Application.Configuration;

public sealed class ExchangeRateOptions
{
    public const string SectionName = "ExchangeRate";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseCurrency { get; set; } = "USD";

    public string TargetCurrency { get; set; } = "PKR";

    public int CacheTtlMinutes { get; set; } = 60;

    public decimal FallbackRate { get; set; } = 280m;
}
