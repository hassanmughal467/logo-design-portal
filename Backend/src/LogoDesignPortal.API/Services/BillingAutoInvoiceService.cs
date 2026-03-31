using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Auto-generates invoices for Weekly (Mondays) and Monthly (1st) clients. Invoked by Hangfire on a daily schedule.
/// </summary>
public class BillingAutoInvoiceService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillingAutoInvoiceService> _logger;
    private readonly ProductionSafetyOptions _safetyOptions;

    public BillingAutoInvoiceService(IServiceProvider serviceProvider, ILogger<BillingAutoInvoiceService> logger, IOptions<ProductionSafetyOptions> safetyOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
    }

    /// <summary>Runs day-filtered invoicing logic once (safe to call daily from Hangfire).</summary>
    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (_safetyOptions.DisableBillingGeneration)
        {
            _logger.LogDebug("Billing auto-invoice skipped: DisableBillingGeneration is enabled.");
            return;
        }

        var now = DateTime.UtcNow;
        var shouldRunWeekly = now.DayOfWeek == DayOfWeek.Monday;
        var shouldRunMonthly = now.Day == 1;

        if (!shouldRunWeekly && !shouldRunMonthly)
            return;

        using var scope = _serviceProvider.CreateScope();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

        Guid? systemUserId = null;
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var superAdminRoleId = await context.Roles
                .Where(r => r.Name == "SuperAdmin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (superAdminRoleId != Guid.Empty)
            {
                var superAdminUserId = await context.Users
                    .Where(u => u.RoleId == superAdminRoleId && !u.IsDeleted)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (superAdminUserId != Guid.Empty)
                    systemUserId = superAdminUserId;
            }
        }
        catch
        {
            // Use null if we can't get SuperAdmin
        }

        await billingService.ProcessAutomaticInvoicingAsync(systemUserId).ConfigureAwait(false);
        _logger.LogInformation("Billing auto-invoice processing completed for {Date}.", now.ToString("yyyy-MM-dd"));
    }
}
