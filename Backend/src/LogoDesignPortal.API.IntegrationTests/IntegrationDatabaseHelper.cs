using LogoDesignPortal.API.IntegrationTests.Support.Factories;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.API.IntegrationTests;

/// <summary>
/// Direct EF seeding helpers for integration scenarios that need specific DB state (completed orders, invoices).
/// Prefer HTTP + public API when possible; use this when setup would otherwise be excessively long.
/// </summary>
public static class IntegrationDatabaseHelper
{
    public static async Task<Guid> InsertCompletedBillableOrderAsync(
        TestWebApplicationFactory factory,
        Guid clientProfileId,
        decimal amount = 150m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = TestOrderFactory.CreateBillableCompleted(clientProfileId, amount);
        db.LogoOrders.Add(order);
        await db.SaveChangesAsync();
        return order.Id;
    }
}
