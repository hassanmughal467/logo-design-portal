using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;

namespace LogoDesignPortal.API.Middleware;

/// <summary>Restricts Hangfire dashboard to SuperAdmin/Admin in non-development environments.</summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        if (http == null)
            return false;

        var env = http.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() == true)
            return true;

        return http.User.Identity?.IsAuthenticated == true
            && (http.User.IsInRole("SuperAdmin") || http.User.IsInRole("Admin"));
    }
}
