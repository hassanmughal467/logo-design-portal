using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LogoDesignPortal.API.Configuration;

/// <summary>
/// Resolves allowed browser origins for CORS and for error/rate-limit responses that bypass the CORS middleware short-circuit.
/// </summary>
public static class CorsAllowedOrigins
{
    public const string PolicyName = "AllowAdmin";

    private static readonly string[] DevelopmentBuiltInOrigins =
    [
        "http://admin.hawkmerchandising.com",
        "https://admin.hawkmerchandising.com",
        "http://api.hawkmerchandising.com",
        "https://api.hawkmerchandising.com",
        "http://localhost:4200",
        "https://localhost:4200",
        "http://127.0.0.1:4200",
        "https://127.0.0.1:4200",
    ];

    private static readonly string[] ProductionBuiltInOrigins =
    [
        "https://admin.hawkmerchandising.com",
        "https://api.hawkmerchandising.com",
    ];

    public static string[] Resolve(IConfiguration configuration, IHostEnvironment? environment = null)
    {
        var fromConfig = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                         ?? Array.Empty<string>();

        if (environment?.IsProduction() == true || environment?.IsEnvironment("Staging") == true)
        {
            var merged = MergeOrigins(ProductionBuiltInOrigins, fromConfig);
            return merged
                .Where(o => o.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        if (fromConfig.Length == 0)
        {
            return DevelopmentBuiltInOrigins;
        }

        return MergeOrigins(DevelopmentBuiltInOrigins, fromConfig);
    }

    private static string[] MergeOrigins(string[] builtIn, string[] fromConfig)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in builtIn)
        {
            if (!string.IsNullOrWhiteSpace(o))
            {
                set.Add(o.Trim());
            }
        }

        foreach (var o in fromConfig)
        {
            if (!string.IsNullOrWhiteSpace(o))
            {
                set.Add(o.Trim());
            }
        }

        return set.Count > 0 ? set.ToArray() : builtIn;
    }

    public static bool IsAllowed(string? origin, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        if (string.IsNullOrEmpty(origin))
        {
            return false;
        }

        foreach (var o in Resolve(configuration, environment))
        {
            if (string.Equals(o, origin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static void AppendHeadersIfAllowed(HttpContext context, IConfiguration configuration)
    {
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        var environment = context.RequestServices.GetService<IHostEnvironment>();
        if (string.IsNullOrEmpty(origin) || !IsAllowed(origin, configuration, environment) || context.Response.HasStarted)
        {
            return;
        }

        context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
    }
}
