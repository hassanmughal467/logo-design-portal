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
}
