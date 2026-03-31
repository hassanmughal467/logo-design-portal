using Serilog.Context;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Assigns a correlation id for the request (header <c>X-Correlation-Id</c> in / out) and pushes it to Serilog log context.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string ItemKey = "CorrelationId";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(incoming)
            ? Guid.NewGuid().ToString("N")[..16]
            : incoming.Trim();

        context.Items[ItemKey] = correlationId;
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
