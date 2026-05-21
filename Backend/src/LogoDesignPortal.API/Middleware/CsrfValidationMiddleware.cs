using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Services;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Double-submit CSRF check when auth cookies are used (mutating requests with cookie session).
/// </summary>
public class CsrfValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthCookieOptions _options;

    public CsrfValidationMiddleware(RequestDelegate next, IOptions<AuthCookieOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IAuthCookieService authCookies)
    {
        if (!_options.Enabled || !authCookies.IsEnabled)
        {
            await _next(context);
            return;
        }

        if (IsSafeMethod(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth/register", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth/refresh-token", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth/forgot-password", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth/reset-password-with-token", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Bearer header auth (e.g. integration tests) — skip CSRF
        if (context.Request.Headers.Authorization.Any(h =>
                h?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue(_options.AccessTokenCookieName, out _))
        {
            await _next(context);
            return;
        }

        context.Request.Cookies.TryGetValue(_options.CsrfCookieName, out var cookieToken);
        var headerToken = context.Request.Headers[_options.CsrfHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken)
            || !string.Equals(cookieToken, headerToken, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "CSRF validation failed." });
            return;
        }

        await _next(context);
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);
}
