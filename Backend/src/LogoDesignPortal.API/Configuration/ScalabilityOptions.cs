namespace LogoDesignPortal.API.Configuration;

/// <summary>
/// Hosting scalability settings. Production normally uses Redis for cache, rate limits, Hangfire, and SignalR scale-out.
/// </summary>
public sealed class ScalabilityOptions
{
    public const string SectionName = "Scalability";

    /// <summary>
    /// When true (single IIS instance / no Redis), the API uses in-memory distributed cache and Hangfire storage like Development.
    /// Do not use with multiple API instances — jobs and rate limits will not be coordinated.
    /// </summary>
    public bool AllowInMemoryFallback { get; set; }

    /// <summary>StackExchange.Redis <see cref="StackExchange.Redis.ConfigurationOptions.ConnectRetry"/>.</summary>
    public int RedisRetryCount { get; set; } = 3;

    /// <summary>Base delay (ms) for <see cref="StackExchange.Redis.ExponentialRetry"/> reconnect policy.</summary>
    public int RedisRetryBaseDelayMs { get; set; } = 500;

    /// <summary>StackExchange.Redis connect timeout in milliseconds.</summary>
    public int RedisConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Ops escape hatch: when Redis is unavailable at request time, rate limiting falls back to in-process counters.
    /// Do not use on multi-instance hosts unless Redis is confirmed down.
    /// </summary>
    public bool EmergencyFallbackEnabled { get; set; }
}
