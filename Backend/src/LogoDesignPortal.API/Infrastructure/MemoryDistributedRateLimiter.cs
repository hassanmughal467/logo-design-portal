using System.Collections.Concurrent;

namespace LogoDesignPortal.API.Infrastructure;

/// <summary>In-process rate limiter when Redis is not used (single-instance only).</summary>
public sealed class MemoryDistributedRateLimiter : IDistributedRateLimiter
{
    private static readonly ConcurrentDictionary<string, Window> Windows = new();

    public Task<(bool Allowed, int RetryAfterSeconds)> TryAcquireAsync(string bucketKey, int maxPerMinute, CancellationToken cancellationToken = default)
    {
        var minute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var key = $"{bucketKey}:{minute}";
        var now = DateTime.UtcNow;
        var reset = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(1);

        var w = Windows.GetOrAdd(key, _ => new Window { ResetUtc = reset });
        lock (w)
        {
            if (now >= w.ResetUtc)
            {
                w.Count = 0;
                w.ResetUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(1);
            }

            w.Count++;
            if (w.Count > maxPerMinute)
            {
                var retry = Math.Max(1, (int)(w.ResetUtc - now).TotalSeconds);
                return Task.FromResult((false, retry));
            }
        }

        return Task.FromResult((true, 0));
    }

    private sealed class Window
    {
        public int Count;
        public DateTime ResetUtc;
    }
}
