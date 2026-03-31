namespace LogoDesignPortal.API.Infrastructure;

/// <summary>Cluster-safe fixed-window counter (Redis) or in-process fallback.</summary>
public interface IDistributedRateLimiter
{
    Task<(bool Allowed, int RetryAfterSeconds)> TryAcquireAsync(string bucketKey, int maxPerMinute, CancellationToken cancellationToken = default);
}
