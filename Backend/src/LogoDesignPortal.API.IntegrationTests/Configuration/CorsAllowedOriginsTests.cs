using LogoDesignPortal.API.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Configuration;

public class CorsAllowedOriginsTests
{
    [Fact]
    public void Resolve_Production_ExcludesLocalhostAndHttpDevOrigins()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://admin.hawkmerchandising.com",
                ["Cors:AllowedOrigins:1"] = "http://localhost:4200"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = Environments.Production };
        var origins = CorsAllowedOrigins.Resolve(config, env);

        Assert.Contains("https://admin.hawkmerchandising.com", origins);
        Assert.Contains("https://api.hawkmerchandising.com", origins);
        Assert.DoesNotContain(origins, o => o.Contains("localhost", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(origins, o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_Development_IncludesLocalhostWhenConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = Environments.Development };
        var origins = CorsAllowedOrigins.Resolve(config, env);

        Assert.Contains(origins, o => o.Contains("localhost", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ProductionAppSettings_CorsContainsOnlyHttpsProductionHosts()
    {
        var path = FindAppSettings("appsettings.Production.json");
        var config = new ConfigurationBuilder().AddJsonFile(path).Build();
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        Assert.All(origins, o => Assert.StartsWith("https://", o, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(origins, o => o.Contains("localhost", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(origins, o => o.Contains("192.168.", StringComparison.OrdinalIgnoreCase));
    }

    private static string FindAppSettings(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            foreach (var prefix in new[] { "LogoDesignPortal.API", Path.Combine("src", "LogoDesignPortal.API") })
            {
                var candidate = Path.Combine(dir, prefix, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException($"Could not locate {fileName}");
    }

    private sealed class HostingEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "LogoDesignPortal.API";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
