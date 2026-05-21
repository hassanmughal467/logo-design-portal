namespace LogoDesignPortal.API.Configuration;

/// <summary>
/// Optional OpenTelemetry / Azure Monitor wiring. Disabled by default; enable per environment via config.
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; }

    /// <summary>OTLP endpoint (Grafana Tempo, Jaeger, etc.). When empty, console exporter is used in Development only.</summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>Azure Application Insights connection string. When set, traces export to Azure Monitor.</summary>
    public string? ApplicationInsightsConnectionString { get; set; }

    /// <summary>Service name resource attribute.</summary>
    public string ServiceName { get; set; } = "LogoDesignPortal.API";
}
