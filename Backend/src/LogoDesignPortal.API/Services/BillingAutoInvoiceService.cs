using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Background service that runs daily to auto-generate invoices for Weekly and Monthly billing clients.
/// Weekly: runs on Mondays, invoices previous week's completed orders.
/// Monthly: runs on 1st of month, invoices previous month's completed orders.
/// Respects ProductionSafety.DisableBillingGeneration kill-switch.
/// </summary>
public class BillingAutoInvoiceService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillingAutoInvoiceService> _logger;
    private readonly ProductionSafetyOptions _safetyOptions;
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    public BillingAutoInvoiceService(IServiceProvider serviceProvider, ILogger<BillingAutoInvoiceService> logger, IOptions<ProductionSafetyOptions> safetyOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BillingAutoInvoiceService started. Will run daily for Weekly/Monthly clients.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Initial delay at startup
                await ProcessAutoInvoicingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Billing auto-invoice processing failed. Will retry at next interval.");
            }

            try
            {
                await Task.Delay(RunInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("BillingAutoInvoiceService stopped.");
    }

    private async Task ProcessAutoInvoicingAsync(CancellationToken cancellationToken)
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
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

        Guid? systemUserId = null;
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var superAdminRoleId = await context.Roles
                .Where(r => r.Name == "SuperAdmin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (superAdminRoleId != Guid.Empty)
            {
                var superAdminUserId = await context.Users
                    .Where(u => u.RoleId == superAdminRoleId && !u.IsDeleted)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (superAdminUserId != Guid.Empty)
                {
                    systemUserId = superAdminUserId;
                }
            }
        }
        catch
        {
            // Use null if we can't get SuperAdmin
        }

        await billingService.ProcessAutomaticInvoicingAsync(systemUserId);
        _logger.LogInformation("Billing auto-invoice processing completed for {Date}.", now.ToString("yyyy-MM-dd"));
    }
}
