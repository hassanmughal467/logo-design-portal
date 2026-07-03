using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Infrastructure.Redis;
using LogoDesignPortal.API.Infrastructure;
using LogoDesignPortal.API.BackgroundJobs;
using LogoDesignPortal.API.Configuration;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.API.Hosting;

public static class ScalabilityServiceRegistration
{
    private static bool AllowsInMemoryFallback(IHostEnvironment environment, IConfiguration configuration) =>
        environment.IsDevelopment()
        || environment.IsEnvironment("Testing")
        || configuration.GetValue($"{ScalabilityOptions.SectionName}:AllowInMemoryFallback", false);

    /// <summary>
    /// Registers Redis-backed cache, shared read-model epochs, and distributed rate limiting when Redis is available.
    /// In Production/Staging without <c>Scalability:AllowInMemoryFallback</c>, Redis is mandatory.
    /// </summary>
    /// <returns>True if Redis was configured and connected.</returns>
    public static bool AddDistributedCacheEpochsAndRateLimiter(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger logger)
    {
        var redis =
            configuration.GetConnectionString("Redis")
            ?? configuration["Redis:Configuration"];

        var redisRequired = !AllowsInMemoryFallback(environment, configuration);

        if (string.IsNullOrWhiteSpace(redis))
        {
            if (redisRequired)
            {
                throw new InvalidOperationException(
                    "Redis is required in non-Development environments (unless Scalability:AllowInMemoryFallback is true for a single server). " +
                    "Set ConnectionStrings:Redis or Redis:Configuration " +
                    "(e.g. localhost:6379 or your Azure/AWS Redis connection string).");
            }

            services.AddDistributedMemoryCache();
            services.AddSingleton<IReadModelCacheVersions, ReadModelCacheVersions>();
            services.AddSingleton<IDistributedRateLimiter, MemoryDistributedRateLimiter>();
            logger.LogWarning("Redis not configured; using in-memory cache, local read-model epochs, and in-memory rate limits (Development only).");
            return false;
        }

        try
        {
            var opts = BuildRedisConfigurationOptions(redis, configuration);
            var mux = ConnectionMultiplexer.Connect(opts);
            services.AddSingleton<IConnectionMultiplexer>(mux);
            services.AddStackExchangeRedisCache(o =>
                o.ConfigurationOptions = BuildRedisConfigurationOptions(redis, configuration));
            services.AddSingleton<MemoryDistributedRateLimiter>();
            services.AddSingleton<IReadModelCacheVersions, RedisReadModelCacheVersions>();
            services.AddSingleton<IDistributedRateLimiter, RedisDistributedRateLimiter>();
            logger.LogInformation("Redis: IDistributedCache, read-model epochs, and rate limiting enabled.");
            return true;
        }
        catch (Exception ex)
        {
            if (redisRequired)
            {
                throw new InvalidOperationException(
                    "Redis is configured but the connection failed. Fix connectivity or credentials, or set Scalability:AllowInMemoryFallback=true only on a single-instance host.",
                    ex);
            }

            logger.LogWarning(ex, "Redis connection failed; using in-memory distributed cache and rate limits (Development only).");
            services.AddDistributedMemoryCache();
            services.AddSingleton<IReadModelCacheVersions, ReadModelCacheVersions>();
            services.AddSingleton<IDistributedRateLimiter, MemoryDistributedRateLimiter>();
            return false;
        }
    }

    public static void AddHangfireForPortal(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        bool redisInfrastructureOk,
        ILogger logger)
    {
        var redis =
            configuration.GetConnectionString("Redis")
            ?? configuration["Redis:Configuration"];

        var useRedisHangfire = redisInfrastructureOk
            && !string.IsNullOrWhiteSpace(redis)
            && !environment.IsEnvironment("Testing");

        if (useRedisHangfire)
        {
            // Shared Redis storage + locks: recurring jobs and background work are coordinated across instances.
            services.AddHangfire((sp, cfg) => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseRedisStorage(redis, new RedisStorageOptions { Prefix = "hangfire:ldp:" }));
        }
        else if (environment.IsEnvironment("Testing") || environment.IsDevelopment() || AllowsInMemoryFallback(environment, configuration))
        {
            services.AddHangfire((sp, cfg) => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage());

            if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            {
                logger.LogWarning(
                    "Hangfire using in-memory storage (Scalability:AllowInMemoryFallback=true). Add Redis before scaling to multiple API instances.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                "Hangfire must use Redis storage when Scalability:AllowInMemoryFallback is false so background jobs are not duplicated across instances.");
        }

        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Math.Clamp(Environment.ProcessorCount, 1, 16);
                // Slightly reduce polling load when many API instances share Redis job storage.
                options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
                options.ServerTimeout = TimeSpan.FromMinutes(4);
                options.ServerCheckInterval = TimeSpan.FromMinutes(2);
            });
        }
    }

    public static ConfigurationOptions BuildRedisConfigurationOptions(string redis, IConfiguration configuration)
    {
        var scalability = configuration.GetSection(ScalabilityOptions.SectionName);
        var connectRetry = scalability.GetValue(nameof(ScalabilityOptions.RedisRetryCount),
            configuration.GetValue("Redis:ConnectRetry", 3));
        var connectTimeout = scalability.GetValue(nameof(ScalabilityOptions.RedisConnectTimeout),
            configuration.GetValue("Redis:ConnectTimeout", 5000));
        var retryBaseDelayMs = scalability.GetValue(nameof(ScalabilityOptions.RedisRetryBaseDelayMs),
            configuration.GetValue("Redis:RetryBaseDelayMs", 500));

        var opts = ConfigurationOptions.Parse(redis);
        opts.AbortOnConnectFail = false;
        opts.ConnectRetry = connectRetry;
        opts.ConnectTimeout = connectTimeout;
        opts.ReconnectRetryPolicy = new ExponentialRetry(retryBaseDelayMs);
        return opts;
    }

    public static void AddRecurringJobsIfEnabled(IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return;
        }

        RecurringJob.AddOrUpdate<MaintenanceHangfireJobs>(
            "orphan-preview-files",
            j => j.RunOrphanCleanupAsync(),
            Cron.Daily());

        RecurringJob.AddOrUpdate<MaintenanceHangfireJobs>(
            "billing-auto-invoice",
            j => j.RunBillingAutoInvoiceAsync(),
            Cron.Daily());
    }
}
