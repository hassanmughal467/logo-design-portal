using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.API.Logging;
using LogoDesignPortal.Application.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Global exception middleware. Catches unhandled exceptions from the pipeline.
/// Returns <c>{ message, correlationId }</c> JSON and completes the response so clients do not hang.
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
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var correlationId = context.GetCorrelationId();
            var category = ExceptionCategoryMapper.Map(ex);
            _logger.LogError(ex,
                "CorrelationId={CorrelationId} ExceptionCategory={ExceptionCategory} | Unhandled exception: {Message} | {Method} {Path}",
                correlationId, category, ex.Message, context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex, correlationId).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError("CorrelationId={CorrelationId} | Response already started; cannot write error body.", correlationId);
            return;
        }

        var (statusCode, message) = GetStatusCodeAndMessage(exception);

        CorsAllowedOrigins.AppendHeadersIfAllowed(context, _configuration);

        // CorrelationIdMiddleware already set this when the response hadn't started.
        if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
        {
            context.Response.Headers.Append("X-Correlation-Id", correlationId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var includeDetailsInProduction = _configuration.GetValue<bool>("IncludeExceptionDetailsInProduction");
        var is500 = statusCode == HttpStatusCode.InternalServerError;

        object response;
        if (_env.IsDevelopment() && is500)
        {
            response = new { message, correlationId, detail = exception.ToString() };
        }
        else if (is500)
        {
            response = includeDetailsInProduction
                ? new { message, correlationId, detail = exception.ToString() }
                : new { message, correlationId };
        }
        else
        {
            response = new { message, correlationId };
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response)).ConfigureAwait(false);
    }

    private static (HttpStatusCode statusCode, string message) GetStatusCodeAndMessage(Exception exception)
    {
        return exception switch
        {
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict,
                "This record was updated by someone else. Refresh the page and try again."),
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
