using StackExchange.Redis;

namespace LogoDesignPortal.API.Infrastructure;

/// <summary>Redis INCR fixed-window limiter shared across API instances. Redis errors propagate to middleware for fail-open / emergency fallback.</summary>
public sealed class RedisDistributedRateLimiter : IDistributedRateLimiter
{
    private readonly IConnectionMultiplexer _mux;

    public RedisDistributedRateLimiter(IConnectionMultiplexer mux)
    {
        _mux = mux;
    }

    public async Task<(bool Allowed, int RetryAfterSeconds)> TryAcquireAsync(string bucketKey, int maxPerMinute, CancellationToken cancellationToken = default)
    {
        var minute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var redisKey = $"ldp:rl:{bucketKey}:{minute}";
        var db = _mux.GetDatabase();
        var count = await db.StringIncrementAsync(redisKey).ConfigureAwait(false);
        if (count == 1)
        {
            await db.KeyExpireAsync(redisKey, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
        }

        if (count > maxPerMinute)
        {
            var ttl = await db.KeyTimeToLiveAsync(redisKey).ConfigureAwait(false);
            var retry = ttl.HasValue && ttl.Value.TotalSeconds > 0 ? (int)ttl.Value.TotalSeconds : 60;
            return (false, retry);
        }

        return (true, 0);
    }
}
