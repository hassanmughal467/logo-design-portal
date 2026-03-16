using LogoDesignPortal.Application.Exceptions;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Global exception middleware. Catches unhandled exceptions from all controllers.
/// Returns safe responses without exposing stack traces to clients.
/// Logs full error details internally for incident tracking.
/// In Development: includes stack trace in 500 response for easier debugging.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log full error details (exception as 1st param = stack trace included in log output)
            _logger.LogError(ex, "Unhandled exception: {Message}. Path: {Path}, Method: {Method}. StackTrace: {StackTrace}",
                ex.Message, context.Request.Path, context.Request.Method, ex.StackTrace);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static readonly string[] AllowedCorsOrigins = new[]
    {
        "https://admin.hawkmerchandising.com",
        "http://admin.hawkmerchandising.com",
        "http://localhost:4200",
        "https://localhost:4200"
    };

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = GetStatusCodeAndMessage(exception);

        // Add CORS headers to error responses so browser doesn't block them (required when ExceptionMiddleware short-circuits pipeline)
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && AllowedCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        object response;
        if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            // In Development: include stack trace so you can see it in browser Network tab
            response = new { message, stackTrace = exception.ToString() };
        }
        else
        {
            response = new { message };
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
