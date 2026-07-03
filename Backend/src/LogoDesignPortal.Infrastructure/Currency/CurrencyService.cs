using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Currency;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Infrastructure.Currency;

public sealed class CurrencyService : ICurrencyService
{
    private const string CacheKey = "forex:usd_pkr";
    private const string LastKnownKey = "forex:usd_pkr:last_known";
    private const string MemoryCacheKey = "forex:usd_pkr:memory";
    private const string MemoryLastKnownKey = "forex:usd_pkr:memory:last_known";

    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ExchangeRateOptions _options;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        HttpClient httpClient,
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        IOptions<ExchangeRateOptions> options,
        ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToPkrRateAsync(CancellationToken cancellationToken = default)
    {
        var info = await GetRateInfoAsync(cancellationToken).ConfigureAwait(false);
        return info.Rate;
    }

    public async Task<ExchangeRateInfo> GetRateInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await TryGetFromDistributedCacheAsync(CacheKey, isStale: false, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }

            var memoryCached = TryGetFromMemoryCache(MemoryCacheKey, isStale: false);
            if (memoryCached != null)
            {
                return memoryCached;
            }

            var live = await TryFetchFromApiAsync(cancellationToken).ConfigureAwait(false);
            if (live != null)
            {
                await PersistRateAsync(live, cancellationToken).ConfigureAwait(false);
                return live;
            }

            var lastKnown = await TryGetFromDistributedCacheAsync(LastKnownKey, isStale: true, cancellationToken).ConfigureAwait(false);
            if (lastKnown != null)
            {
                _logger.LogWarning(
                    "Exchange rate API unavailable; using last-known Redis rate {Rate} fetched at {FetchedAt}",
                    lastKnown.Rate,
                    lastKnown.FetchedAt);
                CacheInMemory(MemoryLastKnownKey, lastKnown);
                return lastKnown;
            }

            var memoryLastKnown = TryGetFromMemoryCache(MemoryLastKnownKey, isStale: true);
            if (memoryLastKnown != null)
            {
                _logger.LogWarning(
                    "Exchange rate API and Redis unavailable; using in-memory last-known rate {Rate} fetched at {FetchedAt}",
                    memoryLastKnown.Rate,
                    memoryLastKnown.FetchedAt);
                return memoryLastKnown;
            }

            _logger.LogError(
                "Exchange rate API and all caches unavailable; using hardcoded fallback rate {Rate}",
                _options.FallbackRate);

            return new ExchangeRateInfo
            {
                Rate = _options.FallbackRate,
                Source = "hardcoded-fallback",
                FetchedAt = DateTime.UtcNow,
                IsStale = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error resolving USD/PKR exchange rate; using hardcoded fallback");

            return new ExchangeRateInfo
            {
                Rate = _options.FallbackRate,
                Source = "hardcoded-fallback",
                FetchedAt = DateTime.UtcNow,
                IsStale = true
            };
        }
    }

    private async Task<ExchangeRateInfo?> TryFetchFromApiAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("ExchangeRate API key is not configured; skipping live rate fetch");
            return null;
        }

        var baseCode = string.IsNullOrWhiteSpace(_options.BaseCurrency) ? "USD" : _options.BaseCurrency.Trim().ToUpperInvariant();
        var targetCode = string.IsNullOrWhiteSpace(_options.TargetCurrency) ? "PKR" : _options.TargetCurrency.Trim().ToUpperInvariant();
        var url = $"https://v6.exchangerate-api.com/v6/{_options.ApiKey.Trim()}/pair/{baseCode}/{targetCode}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Exchange rate API returned HTTP {StatusCode}", (int)response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (payload == null || !string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Exchange rate API returned unsuccessful result: {Result}", payload?.Result);
                return null;
            }

            if (payload.ConversionRate <= 0)
            {
                _logger.LogWarning("Exchange rate API returned invalid conversion rate {Rate}", payload.ConversionRate);
                return null;
            }

            var fetchedAt = payload.TimeLastUpdateUnix > 0
                ? DateTimeOffset.FromUnixTimeSeconds(payload.TimeLastUpdateUnix).UtcDateTime
                : DateTime.UtcNow;

            return new ExchangeRateInfo
            {
                Rate = payload.ConversionRate,
                Source = "exchangerate-api",
                FetchedAt = fetchedAt,
                IsStale = false
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Exchange rate API request timed out after {TimeoutSeconds}s", _httpClient.Timeout.TotalSeconds);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exchange rate API request failed");
            return null;
        }
    }

    private async Task PersistRateAsync(ExchangeRateInfo rate, CancellationToken cancellationToken)
    {
        var ttl = TimeSpan.FromMinutes(Math.Max(1, _options.CacheTtlMinutes));
        var cacheEntry = ToCacheEntry(rate);

        await DistributedJsonCache.SetSafeAsync(_distributedCache, CacheKey, cacheEntry, ttl, _logger, cancellationToken)
            .ConfigureAwait(false);
        await DistributedJsonCache.SetSafeAsync(_distributedCache, LastKnownKey, cacheEntry, _logger, cancellationToken)
            .ConfigureAwait(false);

        CacheInMemory(MemoryCacheKey, rate, ttl);
        CacheInMemory(MemoryLastKnownKey, rate);
    }

    private async Task<ExchangeRateInfo?> TryGetFromDistributedCacheAsync(
        string key,
        bool isStale,
        CancellationToken cancellationToken)
    {
        var entry = await DistributedJsonCache.GetSafeAsync<CachedExchangeRateEntry>(
            _distributedCache,
            key,
            _logger,
            cancellationToken).ConfigureAwait(false);

        return entry == null ? null : FromCacheEntry(entry, isStale);
    }

    private ExchangeRateInfo? TryGetFromMemoryCache(string key, bool isStale)
    {
        return _memoryCache.TryGetValue(key, out ExchangeRateInfo cached) ? cached with { IsStale = isStale } : null;
    }

    private void CacheInMemory(string key, ExchangeRateInfo rate, TimeSpan? ttl = null)
    {
        var options = ttl.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }
            : new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove };

        _memoryCache.Set(key, rate with { IsStale = false }, options);
    }

    private static CachedExchangeRateEntry ToCacheEntry(ExchangeRateInfo rate) =>
        new()
        {
            Rate = rate.Rate,
            Source = rate.Source,
            FetchedAt = rate.FetchedAt
        };

    private static ExchangeRateInfo FromCacheEntry(CachedExchangeRateEntry entry, bool isStale) =>
        new()
        {
            Rate = entry.Rate,
            Source = entry.Source,
            FetchedAt = entry.FetchedAt,
            IsStale = isStale
        };

    private sealed class CachedExchangeRateEntry
    {
        public decimal Rate { get; set; }

        public string Source { get; set; } = string.Empty;

        public DateTime FetchedAt { get; set; }
    }

    private sealed class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rate")]
        public decimal ConversionRate { get; set; }

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }
    }
}
