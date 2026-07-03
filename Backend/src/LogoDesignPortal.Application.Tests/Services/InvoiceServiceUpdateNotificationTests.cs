using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class InvoiceServiceUpdateNotificationTests
{
    [Fact]
    public async Task EditInvoiceItemsAsync_AddOrderIds_NotifiesClientWithNewLogoAndPriceUpdated()
    {
        var (context, client, orders) = await InvoiceServiceTestsHelpers.CreateContextWithEligibleOrdersAsync();
        var notifications = new Mock<INotificationService>();
        var service = InvoiceServiceTestsHelpers.CreateInvoiceService(context, notifications);

        var createResult = await service.GenerateFlexibleInvoiceAsync(new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            SelectedOrderIds = new List<Guid> { orders[0].Id },
            IncludeUninvoicedOnly = true
        }, Guid.NewGuid());

        await service.EditInvoiceItemsAsync(createResult.Id, new EditInvoiceItemsRequestDto
        {
            AddOrderIds = new List<Guid> { orders[1].Id }
        }, Guid.NewGuid());

        notifications.Verify(
            n => n.CreateNotificationAsync(
                client.UserId,
                "Invoice Updated",
                It.Is<string>(m => m.Contains("new logo added", StringComparison.OrdinalIgnoreCase)
                    && m.Contains("price updated", StringComparison.OrdinalIgnoreCase)),
                NotificationType.Info,
                It.IsAny<Guid?>(),
                NotificationReferenceType.Invoice,
                createResult.Id,
                It.IsAny<Guid?>()),
            Times.Once);

        var invoice = await context.Invoices.FirstAsync(i => i.Id == createResult.Id);
        Assert.NotNull(invoice.UpdatedAt);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_ExtendsDueDate_NotifiesClientWithDueDateExtended()
    {
        var (context, client, orders) = await InvoiceServiceTestsHelpers.CreateContextWithEligibleOrdersAsync();
        var notifications = new Mock<INotificationService>();
        var service = InvoiceServiceTestsHelpers.CreateInvoiceService(context, notifications);

        var createResult = await service.GenerateFlexibleInvoiceAsync(new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            SelectedOrderIds = new List<Guid> { orders[0].Id },
            IncludeUninvoicedOnly = true
        }, Guid.NewGuid());

        var originalDue = DateTime.UtcNow.AddDays(5);
        var invoice = await context.Invoices.FirstAsync(i => i.Id == createResult.Id);
        invoice.DueDate = originalDue;
        await context.SaveChangesAsync();

        await service.UpdateInvoiceAsync(createResult.Id, new UpdateInvoiceRequestDto
        {
            DueDate = originalDue.AddDays(14)
        }, Guid.NewGuid());

        notifications.Verify(
            n => n.CreateNotificationAsync(
                client.UserId,
                "Invoice Updated",
                It.Is<string>(m => m.Contains("due date extended", StringComparison.OrdinalIgnoreCase)),
                NotificationType.Info,
                It.IsAny<Guid?>(),
                NotificationReferenceType.Invoice,
                createResult.Id,
                It.IsAny<Guid?>()),
            Times.Once);

        invoice = await context.Invoices.FirstAsync(i => i.Id == createResult.Id);
        Assert.NotNull(invoice.UpdatedAt);
        Assert.True(invoice.UpdatedAt > invoice.CreatedAt);
    }
}

/// <summary>Shared seeding for invoice notification tests (mirrors InvoiceServiceTests setup).</summary>
internal static class InvoiceServiceTestsHelpers
{
    internal static InvoiceService CreateInvoiceService(
        ApplicationDbContext context,
        Mock<INotificationService>? notifications = null)
    {
        return new InvoiceService(
            context,
            new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper(),
            notifications?.Object ?? Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Microsoft.Extensions.Options.Options.Create(new ProductionSafetyOptions()),
            new ReadModelCacheVersions(),
            Mock.Of<ICurrencyService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<InvoiceService>>());
    }

    internal static async Task<(ApplicationDbContext context, ClientProfile client, List<LogoOrder> orders)>
        CreateContextWithEligibleOrdersAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("InvoiceNotify_" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "client@test.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };
        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = clientUser.Id,
            CompanyName = "ACME",
            User = clientUser
        };

        var orders = new List<LogoOrder>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientProfile.Id,
                Title = "Order 1",
                Status = OrderStatus.Completed,
                BillingEligible = true,
                IsInvoiced = false,
                Price = 100,
                ClientChargePrice = 100,
                CompletedDate = new DateTime(2026, 1, 5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientProfile.Id,
                Title = "Order 2",
                Status = OrderStatus.Completed,
                BillingEligible = true,
                IsInvoiced = false,
                Price = 150,
                ClientChargePrice = 150,
                CompletedDate = new DateTime(2026, 1, 20)
            }
        };

        context.Roles.Add(clientRole);
        context.Users.Add(clientUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.AddRange(orders);
        await context.SaveChangesAsync();

        return (context, clientProfile, orders);
    }
}
