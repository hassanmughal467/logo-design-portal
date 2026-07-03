using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Infrastructure;
using LogoDesignPortal.API.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Middleware;

public class RateLimitingMiddlewareResilienceTests
{
    [Fact]
    public async Task InvokeAsync_WhenRedisFailsAndEmergencyFallbackDisabled_AllowsRequest()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            emergencyFallbackEnabled: false,
            onNext: _ => nextCalled = true);

        var context = new DefaultHttpContext
        {
            Request = { Method = "GET", Path = "/api/users" }
        };

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenRedisFailsAndEmergencyFallbackEnabled_UsesMemoryLimiter()
    {
        var failingLimiter = new FailingRedisRateLimiter();
        var nextCalled = false;
        var middleware = CreateMiddleware(
            emergencyFallbackEnabled: true,
            onNext: _ => nextCalled = true,
            limiter: failingLimiter);

        var context = new DefaultHttpContext
        {
            Request = { Method = "GET", Path = "/api/users" }
        };

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(1, failingLimiter.Attempts);
    }

    private static RateLimitingMiddleware CreateMiddleware(
        bool emergencyFallbackEnabled,
        Action<HttpContext> onNext,
        IDistributedRateLimiter? limiter = null)
    {
        limiter ??= new FailingRedisRateLimiter();
        var options = Options.Create(new RateLimitingOptions { GeneralPerMinute = 60 });
        var scalability = Options.Create(new ScalabilityOptions
        {
            EmergencyFallbackEnabled = emergencyFallbackEnabled
        });

        var env = new TestWebHostEnvironment { EnvironmentName = Environments.Production };

        RequestDelegate next = ctx =>
        {
            onNext(ctx);
            return Task.CompletedTask;
        };

        return new RateLimitingMiddleware(
            next,
            NullLogger<RateLimitingMiddleware>.Instance,
            limiter,
            options,
            scalability,
            env,
            new ConfigurationBuilder().Build(),
            new MemoryDistributedRateLimiter());
    }

    private sealed class FailingRedisRateLimiter : IDistributedRateLimiter
    {
        public int Attempts { get; private set; }

        public Task<(bool Allowed, int RetryAfterSeconds)> TryAcquireAsync(
            string bucketKey,
            int maxPerMinute,
            CancellationToken cancellationToken = default)
        {
            Attempts++;
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "LogoDesignPortal.API";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
