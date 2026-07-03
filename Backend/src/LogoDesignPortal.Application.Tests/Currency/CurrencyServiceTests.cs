using System.Net;
using System.Text.Json;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Currency;
using LogoDesignPortal.Infrastructure.Currency;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Currency;

public class CurrencyServiceTests
{
    [Fact]
    public async Task GetRateInfoAsync_ReturnsDistributedCacheHit_WhenFreshEntryExists()
    {
        var distributed = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var fetchedAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        await DistributedJsonCache.SetAsync(distributed, "forex:usd_pkr", new
        {
            Rate = 285.5m,
            Source = "exchangerate-api",
            FetchedAt = fetchedAt
        }, TimeSpan.FromHours(1));

        var service = CreateService(distributed, new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var info = await service.GetRateInfoAsync();

        Assert.Equal(285.5m, info.Rate);
        Assert.Equal("exchangerate-api", info.Source);
        Assert.Equal(fetchedAt, info.FetchedAt);
        Assert.False(info.IsStale);
    }

    [Fact]
    public async Task GetRateInfoAsync_FetchesFromApiAndCaches_WhenCacheMisses()
    {
        var distributed = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                result = "success",
                conversion_rate = 291.25m,
                time_last_update_unix = 1749427200
            }))
        });

        var service = CreateService(distributed, handler, new ExchangeRateOptions { ApiKey = "test-key" });

        var info = await service.GetRateInfoAsync();

        Assert.Equal(291.25m, info.Rate);
        Assert.Equal("exchangerate-api", info.Source);
        Assert.False(info.IsStale);

        var cached = await DistributedJsonCache.GetAsync<ExchangeRateCacheProbe>(distributed, "forex:usd_pkr");
        Assert.NotNull(cached);
        Assert.Equal(291.25m, cached!.Rate);

        var lastKnown = await DistributedJsonCache.GetAsync<ExchangeRateCacheProbe>(distributed, "forex:usd_pkr:last_known");
        Assert.NotNull(lastKnown);
        Assert.Equal(291.25m, lastKnown!.Rate);
    }

    [Fact]
    public async Task GetRateInfoAsync_UsesLastKnownRedisRate_WhenApiFails()
    {
        var distributed = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var fetchedAt = new DateTime(2026, 5, 20, 8, 0, 0, DateTimeKind.Utc);
        await DistributedJsonCache.SetAsync(distributed, "forex:usd_pkr:last_known", new
        {
            Rate = 287.1m,
            Source = "exchangerate-api",
            FetchedAt = fetchedAt
        });

        var service = CreateService(
            distributed,
            new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)),
            new ExchangeRateOptions { ApiKey = "test-key" });

        var info = await service.GetRateInfoAsync();

        Assert.Equal(287.1m, info.Rate);
        Assert.Equal(fetchedAt, info.FetchedAt);
        Assert.True(info.IsStale);
    }

    [Fact]
    public async Task GetRateInfoAsync_ReturnsHardcodedFallback_WhenApiAndCachesUnavailable()
    {
        var service = CreateService(
            new ThrowingDistributedCache(),
            new StubHttpMessageHandler(_ => throw new HttpRequestException("network down")),
            new ExchangeRateOptions { ApiKey = "test-key", FallbackRate = 280m });

        var info = await service.GetRateInfoAsync();

        Assert.Equal(280m, info.Rate);
        Assert.Equal("hardcoded-fallback", info.Source);
        Assert.True(info.IsStale);
    }

    [Fact]
    public async Task GetRateInfoAsync_NeverThrows()
    {
        var service = CreateService(
            new ThrowingDistributedCache(),
            new ThrowingHttpMessageHandler(),
            new ExchangeRateOptions { FallbackRate = 280m });

        var info = await service.GetRateInfoAsync();

        Assert.Equal(280m, info.Rate);
        Assert.True(info.IsStale);
    }

    [Fact]
    public async Task GetUsdToPkrRateAsync_ReturnsRateFromGetRateInfo()
    {
        var distributed = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        await DistributedJsonCache.SetAsync(distributed, "forex:usd_pkr", new
        {
            Rate = 300m,
            Source = "exchangerate-api",
            FetchedAt = DateTime.UtcNow
        }, TimeSpan.FromHours(1));

        var service = CreateService(distributed, new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var rate = await service.GetUsdToPkrRateAsync();

        Assert.Equal(300m, rate);
    }

    private static CurrencyService CreateService(
        IDistributedCache distributedCache,
        HttpMessageHandler handler,
        ExchangeRateOptions? options = null)
    {
        var httpClient = new HttpClient(handler);
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        return new CurrencyService(
            httpClient,
            distributedCache,
            memoryCache,
            Options.Create(options ?? new ExchangeRateOptions { FallbackRate = 280m }),
            NullLogger<CurrencyService>.Instance);
    }

    private sealed class ExchangeRateCacheProbe
    {
        public decimal Rate { get; set; }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("boom");
    }

    private sealed class ThrowingDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) => throw new InvalidOperationException("redis down");

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            throw new InvalidOperationException("redis down");

        public void Refresh(string key) => throw new InvalidOperationException("redis down");

        public Task RefreshAsync(string key, CancellationToken token = default) =>
            throw new InvalidOperationException("redis down");

        public void Remove(string key) => throw new InvalidOperationException("redis down");

        public Task RemoveAsync(string key, CancellationToken token = default) =>
            throw new InvalidOperationException("redis down");

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            throw new InvalidOperationException("redis down");

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
            throw new InvalidOperationException("redis down");
    }
}
