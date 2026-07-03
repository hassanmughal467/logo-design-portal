using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.Infrastructure.Currency;

public static class CurrencyServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExchangeRateOptions>(configuration.GetSection(ExchangeRateOptions.SectionName));
        services.AddMemoryCache();

        services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        return services;
    }
}
