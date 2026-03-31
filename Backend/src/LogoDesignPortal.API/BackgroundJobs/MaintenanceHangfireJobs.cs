using Hangfire;
using LogoDesignPortal.API.Services;

namespace LogoDesignPortal.API.BackgroundJobs;

/// <summary>Scheduled maintenance: orphan preview files and billing runs (single worker when using Redis Hangfire storage).</summary>
public class MaintenanceHangfireJobs
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MaintenanceHangfireJobs(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 600 })]
    public async Task RunOrphanCleanupAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<OrphanFileCleanupService>();
        await svc.RunOnceAsync(CancellationToken.None).ConfigureAwait(false);
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 600 })]
    public async Task RunBillingAutoInvoiceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<BillingAutoInvoiceService>();
        await svc.RunOnceAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
