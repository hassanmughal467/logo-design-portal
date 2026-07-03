namespace LogoDesignPortal.Application.DTOs.Currency;

public sealed record ExchangeRateInfo
{
    public decimal Rate { get; init; }

    public string Source { get; init; } = string.Empty;

    public DateTime FetchedAt { get; init; }

    public bool IsStale { get; init; }
}
