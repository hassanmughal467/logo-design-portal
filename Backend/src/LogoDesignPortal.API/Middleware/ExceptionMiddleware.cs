using LogoDesignPortal.Application.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Global exception middleware. Catches unhandled exceptions from all controllers.
/// Returns safe responses without exposing stack traces to clients.
/// Logs full error details internally for incident tracking.
/// Includes errorId in response for easy log correlation - search server logs for the errorId to find full stack trace.
/// In Development: includes stack trace in 500 response for easier debugging.
/// Set "IncludeExceptionDetailsInProduction": true in appsettings to include exception message in production (for debugging).
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid().ToString("N")[..12];
            // Log with errorId first so you can search logs for it when user reports the errorId from API response
            _logger.LogError(ex,
                "ErrorId: {ErrorId} | Unhandled exception: {Message} | Path: {Path} | Method: {Method} | StackTrace: {StackTrace}",
                errorId, ex.Message, context.Request.Path, context.Request.Method, ex.StackTrace);
            await HandleExceptionAsync(context, ex, errorId);
        }
    }

    private static readonly string[] AllowedCorsOrigins = new[]
    {
        "https://admin.hawkmerchandising.com",
        "http://admin.hawkmerchandising.com",
        "https://api.hawkmerchandising.com",
        "http://api.hawkmerchandising.com",
        "http://localhost:4200",
        "https://localhost:4200"
    };

    private Task HandleExceptionAsync(HttpContext context, Exception exception, string errorId)
    {
        var (statusCode, message) = GetStatusCodeAndMessage(exception);

        // Add CORS headers to error responses so browser doesn't block them (required when ExceptionMiddleware short-circuits pipeline)
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && AllowedCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        }

        // Add errorId to response header for easy tracking (e.g., in IIS logs, browser DevTools)
        context.Response.Headers.Append("X-Error-Id", errorId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var includeDetailsInProduction = _configuration.GetValue<bool>("IncludeExceptionDetailsInProduction");
        var is500 = statusCode == HttpStatusCode.InternalServerError;

        object response;
        if (_env.IsDevelopment() && is500)
        {
            response = new { message, errorId, ex = exception.ToString() };
        }
        else if (is500)
        {
            // In production: always include errorId. When IncludeExceptionDetailsInProduction is true, include full exception (type, message, stack trace, inner exceptions).
            response = includeDetailsInProduction
                ? new { message, errorId, ex = exception.ToString() }
                : new { message, errorId };
        }
        else
        {
            response = new { message, errorId };
        }

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode statusCode, string message) GetStatusCodeAndMessage(Exception exception)
    {
        return exception switch
        {
            ForbiddenAccessException => (HttpStatusCode.Forbidden, exception.Message),
            UnauthorizedAccessException uaEx when uaEx.Message?.Contains("Access to the path", StringComparison.OrdinalIgnoreCase) == true
                || uaEx.Message?.Contains("is denied", StringComparison.OrdinalIgnoreCase) == true =>
                (HttpStatusCode.InternalServerError, "File storage access denied. Please contact support."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            FileNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please contact support.")
        };
    }
}
