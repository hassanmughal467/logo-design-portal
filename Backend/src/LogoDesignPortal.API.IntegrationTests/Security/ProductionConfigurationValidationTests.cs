using LogoDesignPortal.API.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

/// <summary>
/// Fail-fast production configuration and deployment-safety checks (deterministic, no live DB).
/// </summary>
public class ProductionConfigurationValidationTests
{
    [Fact]
    public void Validate_ProductionWithPlaceholderJwt_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong!",
                ["IncludeExceptionDetailsInProduction"] = "false"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = Environments.Production };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsValidator.Validate(config, env));

        Assert.Contains("placeholder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ProductionWithExceptionDetailsEnabled_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = new string('x', 40),
                ["IncludeExceptionDetailsInProduction"] = "true"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = Environments.Production };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsValidator.Validate(config, env));

        Assert.Contains("IncludeExceptionDetailsInProduction", ex.Message);
    }

    [Fact]
    public void Validate_StagingWithExceptionDetailsEnabled_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = new string('y', 40),
                ["IncludeExceptionDetailsInProduction"] = "true"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = "Staging" };

        Assert.Throws<InvalidOperationException>(() =>
            ProductionSecretsValidator.Validate(config, env));
    }

    [Fact]
    public void Validate_DevelopmentWithPlaceholderJwt_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong!"
            })
            .Build();

        var env = new HostingEnvironment { EnvironmentName = Environments.Development };

        ProductionSecretsValidator.Validate(config, env);
    }

    [Fact]
    public void ProductionAppSettings_DisablesRunAfterStartupMigration()
    {
        var apiRoot = FindApiProjectDirectory();
        var productionPath = Path.Combine(apiRoot, "appsettings.Production.json");
        Assert.True(File.Exists(productionPath), $"Missing {productionPath}");

        var config = new ConfigurationBuilder()
            .AddJsonFile(productionPath, optional: false)
            .Build();

        Assert.False(config.GetValue("Database:RunAfterStartup", true));
    }

    [Fact]
    public void ProductionAppSettings_RestrictsAllowedHosts()
    {
        var config = LoadProductionConfig();
        var hosts = config["AllowedHosts"];
        Assert.False(string.Equals(hosts, "*", StringComparison.Ordinal));
        Assert.Contains("hawkmerchandising.com", hosts, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StagingAppSettings_RequiresJwtFromEnvironment_NotCommittedPlaceholder()
    {
        var apiRoot = FindApiProjectDirectory();
        var stagingPath = Path.Combine(apiRoot, "appsettings.Staging.json");
        Assert.True(File.Exists(stagingPath));

        var raw = File.ReadAllText(stagingPath);
        Assert.DoesNotContain("YourSuperSecretKeyForJWTTokenGeneration", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionAppSettings_DisablesExceptionDetails()
    {
        var apiRoot = FindApiProjectDirectory();
        var productionPath = Path.Combine(apiRoot, "appsettings.Production.json");

        var config = new ConfigurationBuilder()
            .AddJsonFile(productionPath, optional: false)
            .Build();

        Assert.False(config.GetValue("IncludeExceptionDetailsInProduction", true));
        Assert.False(config.GetValue("Scalability:AllowInMemoryFallback", true));
    }

    private static IConfiguration LoadProductionConfig()
    {
        var apiRoot = FindApiProjectDirectory();
        return new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiRoot, "appsettings.Production.json"), optional: false)
            .Build();
    }

    private static string FindApiProjectDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "LogoDesignPortal.API", "appsettings.Production.json");
            if (File.Exists(candidate))
                return Path.Combine(dir, "LogoDesignPortal.API");

            var srcCandidate = Path.Combine(dir, "src", "LogoDesignPortal.API", "appsettings.Production.json");
            if (File.Exists(srcCandidate))
                return Path.Combine(dir, "src", "LogoDesignPortal.API");

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate LogoDesignPortal.API appsettings.Production.json");
    }

    private sealed class HostingEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "LogoDesignPortal.API";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
