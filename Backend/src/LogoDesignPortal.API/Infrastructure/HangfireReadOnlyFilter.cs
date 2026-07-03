using Hangfire.Dashboard;

namespace LogoDesignPortal.API.Infrastructure;

/// <summary>Admins view jobs read-only; SuperAdmins can trigger, requeue, and delete.</summary>
public static class HangfireReadOnlyFilter
{
    public static bool IsReadOnly(DashboardContext context) =>
        !context.GetHttpContext().User.IsInRole("SuperAdmin");
}
