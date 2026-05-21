using LogoDesignPortal.API.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Configuration;

/// <summary>
/// Validates per-environment appsettings structure and deployment-safety defaults (no live DB/Redis).
/// </summary>
public class EnvironmentAppSettingsValidationTests
{
    public static TheoryData<string> DeployedEnvironments => new()
    {
        "Staging",
        "Production"
    };

    [Theory]
    [InlineData("Development", "Files_Dev")]
    [InlineData("Testing", "Files_Test")]
    [InlineData("Staging", "Files_Staging")]
    [InlineData("Production", "Files")]
    public void AppSettings_FileStoragePath_IsolatedPerEnvironment(string envName, string expectedPath)
    {
        var config = LoadMergedConfiguration(envName);
        Assert.Equal(expectedPath, config["FileStorage:Path"], ignoreCase: true);
    }

    [Theory]
    [MemberData(nameof(DeployedEnvironments))]
    public void AppSettings_DeployedEnvironment_DisablesRunAfterStartupMigration(string envName)
    {
        var config = LoadMergedConfiguration(envName);
        Assert.False(config.GetValue("Database:RunAfterStartup", true));
    }

    [Fact]
    public void StagingAppSettings_DisablesExceptionDetails()
    {
        var config = LoadMergedConfiguration("Staging");
        Assert.False(config.GetValue("IncludeExceptionDetailsInProduction", true));
    }

    [Fact]
    public void StagingAppSettings_DisablesBillingKillSwitch()
    {
        var config = LoadMergedConfiguration("Staging");
        Assert.True(config.GetValue("ProductionSafety:DisableBillingGeneration", false));
        Assert.True(config.GetValue("ProductionSafety:DisableDesignerPayout", false));
    }

    [Fact]
    public void StagingAppSettings_UsesTestPayments()
    {
        var config = LoadMergedConfiguration("Staging");
        Assert.True(config.GetValue("Payments:UseTestMode", false));
    }

    [Fact]
    public void ProductionAppSettings_DisablesTestPayments()
    {
        var config = LoadMergedConfiguration("Production");
        Assert.False(config.GetValue("Payments:UseTestMode", true));
    }

    [Fact]
    public void DevelopmentAppSettings_DoesNotCommitDatabasePassword()
    {
        var config = LoadMergedConfiguration("Development");
        var cs = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        Assert.DoesNotContain("ADMIN", cs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("REPLACE_IN_USER_SECRETS_OR_ENV", cs, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(DeployedEnvironments))]
    public void Validate_DeployedWithPlaceholderConnectionString_Throws(string envName)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "REPLACE_IN_ENV_ConnectionStrings__DefaultConnection",
                ["FileStorage:Path"] = envName == "Staging" ? "Files_Staging" : "Files",
                ["Jwt:Key"] = new string('z', 40)
            })
            .Build();

        var env = new TestHostEnvironment { EnvironmentName = envName };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            EnvironmentConfigurationValidator.Validate(config, env));

        Assert.Contains("placeholder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_StagingWithWrongFileStoragePath_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=x;User=u;Password=validpassword;",
                ["FileStorage:Path"] = "Files"
            })
            .Build();

        var env = new TestHostEnvironment { EnvironmentName = "Staging" };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            EnvironmentConfigurationValidator.Validate(config, env));

        Assert.Contains("Files_Staging", ex.Message);
    }

    private static IConfiguration LoadMergedConfiguration(string environmentName)
    {
        var apiRoot = FindApiProjectDirectory();
        var builder = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiRoot, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiRoot, $"appsettings.{environmentName}.json"), optional: true);

        return builder.Build();
    }

    private static string FindApiProjectDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "LogoDesignPortal.API", "appsettings.json");
            if (File.Exists(candidate))
                return Path.Combine(dir, "LogoDesignPortal.API");

            var srcCandidate = Path.Combine(dir, "src", "LogoDesignPortal.API", "appsettings.json");
            if (File.Exists(srcCandidate))
                return Path.Combine(dir, "src", "LogoDesignPortal.API");

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate LogoDesignPortal.API appsettings.json");
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "LogoDesignPortal.API";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
