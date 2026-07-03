using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.API.Infrastructure;

/// <summary>Restricts Hangfire dashboard to Admin and SuperAdmin roles.</summary>
public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var user = http.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole("SuperAdmin") || user.IsInRole("Admin"))
        {
            return true;
        }

        LogNonAdminAttempt(http, user);
        return false;
    }

    internal static void LogNonAdminAttempt(HttpContext http, ClaimsPrincipal user)
    {
        var logger = http.RequestServices.GetRequiredService<ILogger<HangfireAuthorizationFilter>>();
        logger.LogWarning(
            "User {UserName} denied access to Hangfire dashboard at {Path}",
            user.Identity?.Name ?? "unknown",
            http.Request.Path);
    }
}

/// <summary>
/// Returns 403 for authenticated non-admin users before Hangfire (which maps denied auth to 401).
/// </summary>
public sealed class HangfireDashboardAccessMiddleware
{
    private readonly RequestDelegate _next;

    public HangfireDashboardAccessMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase)
            && context.User.Identity?.IsAuthenticated == true
            && !context.User.IsInRole("SuperAdmin")
            && !context.User.IsInRole("Admin"))
        {
            HangfireAuthorizationFilter.LogNonAdminAttempt(context, context.User);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }
}
