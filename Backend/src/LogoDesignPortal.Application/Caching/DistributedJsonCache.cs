using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Caching;

/// <summary>
/// JSON serialization helpers for <see cref="IDistributedCache"/> (Redis or memory fallback).
/// Keys should include epoch and request params; TTLs match former MemoryCache behavior.
/// </summary>
public static class DistributedJsonCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task<T?> GetAsync<T>(IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (bytes == null || bytes.Length == 0)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public static async Task SetAsync<T>(IDistributedCache cache, string key, T value, TimeSpan absoluteTtl, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteTtl
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Cache read that does not throw when Redis or serialization fails (fail-open).</summary>
    public static async Task<T?> GetSafeAsync<T>(IDistributedCache cache, string key, Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<T>(cache, key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Distributed cache read failed for key {CacheKey}", key);
            return default;
        }
    }

    /// <summary>Cache write that logs and ignores failures (fail-open).</summary>
    public static async Task SetSafeAsync<T>(IDistributedCache cache, string key, T value, TimeSpan absoluteTtl, Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAsync(cache, key, value, absoluteTtl, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Distributed cache write failed for key {CacheKey}", key);
        }
    }

    public static async Task SetAsync<T>(IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await cache.SetAsync(key, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Cache write without expiration that logs and ignores failures (fail-open).</summary>
    public static async Task SetSafeAsync<T>(IDistributedCache cache, string key, T value, Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAsync(cache, key, value, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Distributed cache write failed for key {CacheKey}", key);
        }
    }
}
