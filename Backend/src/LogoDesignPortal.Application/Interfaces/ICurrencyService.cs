using LogoDesignPortal.Application.DTOs.Currency;

namespace LogoDesignPortal.Application.Interfaces;

public interface ICurrencyService
{
    Task<decimal> GetUsdToPkrRateAsync(CancellationToken cancellationToken = default);

    Task<ExchangeRateInfo> GetRateInfoAsync(CancellationToken cancellationToken = default);
}
