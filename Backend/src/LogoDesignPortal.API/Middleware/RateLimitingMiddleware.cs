using System.Collections.Concurrent;
using System.Net;

namespace LogoDesignPortal.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
    private const int MaxRequestsPerMinute = 60;
    private const int MaxAuthRequestsPerMinute = 5; // Stricter for auth endpoints

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for CORS preflight requests
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value?.ToLower() ?? "";
        var isAuthEndpoint = path.Contains("/api/auth/") || path.Contains("/api/auth/login") || path.Contains("/api/auth/register");
        var maxRequests = isAuthEndpoint ? MaxAuthRequestsPerMinute : MaxRequestsPerMinute;

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{clientIp}:{path}";

        var now = DateTime.UtcNow;
        var rateLimitInfo = _requestCounts.GetOrAdd(key, _ => new RateLimitInfo { ResetTime = now.AddMinutes(1) });

        // Reset if time window has passed
        if (now > rateLimitInfo.ResetTime)
        {
            rateLimitInfo.Count = 0;
            rateLimitInfo.ResetTime = now.AddMinutes(1);
        }

        // Check rate limit
        if (rateLimitInfo.Count >= maxRequests)
        {
            _logger.LogWarning($"Rate limit exceeded for {clientIp} on {path}");
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Rate limit exceeded. Please try again later.",
                retryAfter = (int)(rateLimitInfo.ResetTime - now).TotalSeconds
            }));
            return;
        }

        rateLimitInfo.Count++;
        await _next(context);
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
