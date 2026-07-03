using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>
/// Fails readiness until EF migrations are applied and core tables exist. Prevents load balancers from
/// sending traffic before the schema is usable.
/// </summary>
public sealed class DatabaseSchemaReadinessHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseSchemaReadinessHealthCheck> _logger;

    public DatabaseSchemaReadinessHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseSchemaReadinessHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!db.Database.IsRelational())
        {
            return HealthCheckResult.Healthy("Non-relational provider.");
        }

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        if (pending.Any())
        {
            return HealthCheckResult.Unhealthy(
                "Database has pending EF Core migrations: " + string.Join(", ", pending));
        }

        // Constant SQL only (no user input) — satisfies EF1002 and works for MySQL + SQLite.
        var probeStatements = new[]
        {
            "SELECT 1 FROM LogoOrders LIMIT 1",
            "SELECT 1 FROM Invoices LIMIT 1",
            "SELECT 1 FROM Payments LIMIT 1",
            "SELECT 1 FROM Users LIMIT 1",
            "SELECT 1 FROM __EFMigrationsHistory LIMIT 1"
        };

        try
        {
            foreach (var sql in probeStatements)
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness: required table probe failed.");
            return HealthCheckResult.Unhealthy("Required tables are missing or the schema is incomplete.");
        }

        return HealthCheckResult.Healthy("Migrations applied; core tables exist.");
    }
}
