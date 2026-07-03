using LogoDesignPortal.API.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Ensures browser CORS preflight (OPTIONS) succeeds when IIS modules would otherwise handle OPTIONS before ASP.NET Core.
/// </summary>
public sealed class CorsPreflightMiddleware
{
    private readonly RequestDelegate _next;

    public CorsPreflightMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (!HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrEmpty(origin) || !CorsAllowedOrigins.IsAllowed(origin, configuration, environment))
        {
            await _next(context);
            return;
        }

        context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        var requestedHeaders = context.Request.Headers.AccessControlRequestHeaders.FirstOrDefault();
        if (!string.IsNullOrEmpty(requestedHeaders))
        {
            context.Response.Headers.Append("Access-Control-Allow-Headers", requestedHeaders);
        }
        else
        {
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-XSRF-TOKEN, X-Requested-With");
        }

        var requestedMethod = context.Request.Headers.AccessControlRequestMethod.FirstOrDefault();
        context.Response.Headers.Append("Access-Control-Allow-Methods",
            string.IsNullOrEmpty(requestedMethod) ? "GET, POST, PUT, PATCH, DELETE, OPTIONS" : requestedMethod);
        context.Response.Headers.Append("Access-Control-Max-Age", "86400");
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }
}
