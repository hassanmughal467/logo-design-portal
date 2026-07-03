using LogoDesignPortal.API.IntegrationTests.Support.Factories;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        _ = factory.CreateClient();
        var resolvedClientProfileId = await ResolveClientProfileIdAsync(factory, clientProfileId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = TestOrderFactory.CreateBillableCompleted(resolvedClientProfileId, amount);
        order.CreatedAt = DateTime.UtcNow;
        db.LogoOrders.Add(order);
        await db.SaveChangesAsync();
        return order.Id;
    }

    private static async Task<Guid> ResolveClientProfileIdAsync(
        TestWebApplicationFactory factory,
        Guid clientProfileId)
    {
        if (clientProfileId != Guid.Empty)
        {
            return clientProfileId;
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            var id = await db.ClientProfiles
                .Where(c => c.User!.Email == "client@test.com")
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
            if (id != Guid.Empty)
            {
                TestDataIds.ClientProfileId = id;
                return id;
            }

            await Task.Delay(100);
        }

        throw new InvalidOperationException(
            "Seeded client profile not found. Ensure TestDataSeeder completed before inserting orders.");
    }

    public static async Task<Guid> InsertPreviewDeliveredOrderAsync(
        TestWebApplicationFactory factory,
        Guid clientProfileId,
        decimal price = 80m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = TestOrderFactory.CreatePreviewDelivered(clientProfileId, price);
        db.LogoOrders.Add(order);
        await db.SaveChangesAsync();
        return order.Id;
    }

    /// <summary>Invoice owned by a different client profile (not the seeded client@test.com).</summary>
    public static async Task<Guid> InsertOtherClientInvoiceAsync(
        TestWebApplicationFactory factory,
        decimal totalAmount = 99m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clientRole = await db.Roles.FirstAsync(r => r.Name == "Client");

        var otherUserId = Guid.NewGuid();
        var otherProfileId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = otherUserId,
            Email = $"other-client-{otherUserId:N}@test.com",
            FirstName = "Other",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = otherProfileId,
            UserId = otherUserId,
            CompanyName = "Other Co",
            CreatedAt = DateTime.UtcNow
        });

        var invoiceId = Guid.NewGuid();
        db.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            ClientId = otherProfileId,
            InvoiceNumber = $"INV-OTHER-{invoiceId:N}"[..20],
            Amount = totalAmount,
            TaxAmount = 0m,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Pending,
            BillingType = BillingType.PerLogo,
            IssueDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return invoiceId;
    }
}
