using System.Diagnostics;
using System.Security.Claims;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Logs warnings when total request time exceeds a threshold (targets &lt; 500ms API SLA).
/// </summary>
public sealed class SlowRequestPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SlowRequestPerformanceMiddleware> _logger;

    public SlowRequestPerformanceMiddleware(RequestDelegate next, ILogger<SlowRequestPerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            if (elapsedMs >= 500)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogWarning(
                    "Slow request: {Method} {Path} completed in {ElapsedMs:F0} ms (SLO target 500 ms). Status={StatusCode}. UserId={UserId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    elapsedMs,
                    context.Response.StatusCode,
                    string.IsNullOrEmpty(userId) ? null : userId);
            }
        }
    }
}
