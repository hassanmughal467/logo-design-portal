using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LogoDesignPortal.API.Infrastructure;

/// <summary>Redis INCR fixed-window limiter shared across API instances. Fails open when Redis errors.</summary>
public sealed class RedisDistributedRateLimiter : IDistributedRateLimiter
{
    private readonly IConnectionMultiplexer _mux;
    private readonly ILogger<RedisDistributedRateLimiter> _logger;

    public RedisDistributedRateLimiter(IConnectionMultiplexer mux, ILogger<RedisDistributedRateLimiter> logger)
    {
        _mux = mux;
        _logger = logger;
    }

    public async Task<(bool Allowed, int RetryAfterSeconds)> TryAcquireAsync(string bucketKey, int maxPerMinute, CancellationToken cancellationToken = default)
    {
        try
        {
            var minute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var redisKey = $"ldp:rl:{bucketKey}:{minute}";
            var db = _mux.GetDatabase();
            var count = await db.StringIncrementAsync(redisKey).ConfigureAwait(false);
            if (count == 1)
                await db.KeyExpireAsync(redisKey, TimeSpan.FromMinutes(2)).ConfigureAwait(false);

            if (count > maxPerMinute)
            {
                var ttl = await db.KeyTimeToLiveAsync(redisKey).ConfigureAwait(false);
                var retry = ttl.HasValue && ttl.Value.TotalSeconds > 0 ? (int)ttl.Value.TotalSeconds : 60;
                return (false, retry);
            }

            return (true, 0);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Rate limit Redis error; allowing request (fail-open). Bucket={Bucket}", bucketKey);
            return (true, 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rate limit unexpected error; allowing request (fail-open). Bucket={Bucket}", bucketKey);
            return (true, 0);
        }
    }
}
