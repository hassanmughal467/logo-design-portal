using LogoDesignPortal.API.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Hosting;

/// <summary>
/// Deployment safety: controlled database initialization at startup.
/// </summary>
public class DatabaseInitializationSafetyTests
{
    [Fact]
    public void DatabaseInitializationOptions_DefaultsToRunAfterStartupTrue()
    {
        var options = new DatabaseInitializationOptions();
        Assert.True(options.RunAfterStartup);
    }

    [Fact]
    public void ProductionConfiguration_RecommendsRunAfterStartupFalse()
    {
        var config = LoadProductionAppSettings();
        Assert.False(config.GetValue("Database:RunAfterStartup", defaultValue: true));
    }

    [Fact]
    public void StagingAppSettings_DoesNotExposeExceptionDetails()
    {
        var path = FindAppSettings("appsettings.Staging.json");
        var config = new ConfigurationBuilder().AddJsonFile(path).Build();
        Assert.False(config.GetValue("IncludeExceptionDetailsInProduction", defaultValue: true));
    }

    private static IConfiguration LoadProductionAppSettings()
    {
        var path = FindAppSettings("appsettings.Production.json");
        return new ConfigurationBuilder().AddJsonFile(path).Build();
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
                    return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException($"Could not locate {fileName}");
    }
}
