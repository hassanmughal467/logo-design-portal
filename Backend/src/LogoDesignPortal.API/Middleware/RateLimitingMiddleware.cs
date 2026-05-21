using System.Net;
using System.Security.Claims;
using System.Text.Json;
using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.API.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Cluster-aware rate limiting when Redis is registered; otherwise in-process.
/// Sensitive routes use stricter buckets (auth, order mutations, file uploads).
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IDistributedRateLimiter _limiter;
    private readonly RateLimitingOptions _options;
    private readonly bool _disabled;
    private readonly IConfiguration _configuration;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IDistributedRateLimiter limiter,
        IOptions<RateLimitingOptions> options,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _limiter = limiter;
        _options = options.Value;
        _configuration = configuration;
        _disabled = env.IsDevelopment()
            || env.IsEnvironment("Testing")
            || string.Equals(Environment.GetEnvironmentVariable("DISABLE_RATE_LIMIT"), "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_disabled)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        if (context.Request.Method == "OPTIONS")
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.StartsWith("/health", StringComparison.Ordinal) ||
            path.StartsWith("/swagger", StringComparison.Ordinal))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }
        var method = context.Request.Method;

        var (bucket, limit) = Classify(path, method, context);
        if (limit <= 0)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var identity = ResolveIdentity(context, bucket);
        var compositeKey = $"{bucket}:{identity}";
        var (allowed, retry) = await _limiter.TryAcquireAsync(compositeKey, limit, context.RequestAborted).ConfigureAwait(false);

        if (!allowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded bucket={Bucket} identity={Identity} path={Path} correlationId={CorrelationId}",
                bucket, identity, path, context.GetCorrelationId());
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Append("Retry-After", Math.Max(1, retry).ToString());
            CorsAllowedOrigins.AppendHeadersIfAllowed(context, _configuration);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "Rate limit exceeded. Please try again later.",
                correlationId = context.GetCorrelationId(),
                retryAfterSeconds = retry
            })).ConfigureAwait(false);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private (string Bucket, int Limit) Classify(string path, string method, HttpContext context)
    {
        if (path.Contains("/api/auth/login") || path.Contains("/api/auth/register") || path.Contains("/api/auth/forgot-password"))
            return ("auth", _options.AuthPerMinute);

        if (!HttpMethods.IsGet(method) && path.StartsWith("/api/orders", StringComparison.Ordinal))
            return ("orders", _options.OrdersMutationsPerMinute);

        if (HttpMethods.IsPost(method) && path.StartsWith("/api/files", StringComparison.Ordinal))
        {
            var ct = context.Request.ContentType ?? "";
            if (ct.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                return ("files", _options.FileUploadPerMinute);
        }

        return ("general", _options.GeneralPerMinute);
    }

    private static string ResolveIdentity(HttpContext context, string bucket)
    {
        if (bucket is "orders" or "files")
        {
            var uid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(uid))
                return $"u:{uid}";
        }

        return $"ip:{context.GetRateLimitClientId()}";
    }
}
