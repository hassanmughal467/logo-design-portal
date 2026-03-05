using LogoDesignPortal.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace LogoDesignPortal.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ForbiddenAccessException:
                code = HttpStatusCode.Forbidden;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            case UnauthorizedAccessException uaEx:
                if (uaEx.Message?.Contains("Access to the path", StringComparison.OrdinalIgnoreCase) == true ||
                    uaEx.Message?.Contains("is denied", StringComparison.OrdinalIgnoreCase) == true)
                {
                    code = HttpStatusCode.InternalServerError;
                    result = JsonSerializer.Serialize(new { error = "File storage access denied. Ensure the application has write permissions to the Files directory. See server logs for details." });
                }
                else
                {
                    code = HttpStatusCode.Unauthorized;
                    result = JsonSerializer.Serialize(new { error = exception.Message });
                }
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            default:
                var errorDetail = exception.InnerException?.Message ?? exception.Message;
                result = JsonSerializer.Serialize(new { error = "An error occurred while processing your request.", detail = errorDetail });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
