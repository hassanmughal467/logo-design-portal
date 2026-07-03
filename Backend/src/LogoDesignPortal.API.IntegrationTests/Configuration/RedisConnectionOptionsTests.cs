using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Hosting;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Configuration;

public class RedisConnectionOptionsTests
{
    [Fact]
    public void BuildRedisConfigurationOptions_AppliesScalabilitySettings()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ScalabilityOptions.SectionName}:{nameof(ScalabilityOptions.RedisRetryCount)}"] = "5",
                [$"{ScalabilityOptions.SectionName}:{nameof(ScalabilityOptions.RedisConnectTimeout)}"] = "9000",
                [$"{ScalabilityOptions.SectionName}:{nameof(ScalabilityOptions.RedisRetryBaseDelayMs)}"] = "750",
            })
            .Build();

        var opts = ScalabilityServiceRegistration.BuildRedisConfigurationOptions("localhost:6379", config);

        Assert.False(opts.AbortOnConnectFail);
        Assert.Equal(5, opts.ConnectRetry);
        Assert.Equal(9000, opts.ConnectTimeout);
        Assert.IsType<ExponentialRetry>(opts.ReconnectRetryPolicy);
    }

    [Fact]
    public void BuildRedisConfigurationOptions_FallsBackToRedisSectionKeys()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:ConnectRetry"] = "2",
                ["Redis:ConnectTimeout"] = "4000",
                ["Redis:RetryBaseDelayMs"] = "250",
            })
            .Build();

        var opts = ScalabilityServiceRegistration.BuildRedisConfigurationOptions("localhost:6379", config);

        Assert.Equal(2, opts.ConnectRetry);
        Assert.Equal(4000, opts.ConnectTimeout);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void AppSettings_DeployedEnvironment_HasRedisResilienceDefaults(string envName)
    {
        var apiRoot = FindApiProjectDirectory();
        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiRoot, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiRoot, $"appsettings.{envName}.json"), optional: true)
            .Build();

        Assert.False(config.GetValue($"{ScalabilityOptions.SectionName}:AllowInMemoryFallback", true));
        Assert.Equal(3, config.GetValue($"{ScalabilityOptions.SectionName}:RedisRetryCount", 0));
        Assert.Equal(500, config.GetValue($"{ScalabilityOptions.SectionName}:RedisRetryBaseDelayMs", 0));
        Assert.Equal(5000, config.GetValue($"{ScalabilityOptions.SectionName}:RedisConnectTimeout", 0));
        Assert.False(config.GetValue($"{ScalabilityOptions.SectionName}:EmergencyFallbackEnabled", true));
    }

    private static string FindApiProjectDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "LogoDesignPortal.API", "appsettings.json");
            if (File.Exists(candidate))
            {
                return Path.Combine(dir, "LogoDesignPortal.API");
            }

            var srcCandidate = Path.Combine(dir, "src", "LogoDesignPortal.API", "appsettings.json");
            if (File.Exists(srcCandidate))
            {
                return Path.Combine(dir, "src", "LogoDesignPortal.API");
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate LogoDesignPortal.API appsettings.json");
    }
}
