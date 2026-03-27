using AutoMapper;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class InvoiceServiceBranchTests
{
    [Fact]
    public async Task CreateInvoiceAsync_Success_UsesLatestDuplicateOrderPriceAndSendsRealtime()
    {
        var fixture = await SeedAsync();
        var request = new CreateInvoiceRequestDto
        {
            Orders = new()
            {
                new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 100m },
                new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 140m } // last wins
            },
            BillingType = BillingType.PerLogo,
            TaxAmount = 10m
        };

        var result = await fixture.Service.CreateInvoiceAsync(request, fixture.AdminUserId);

        Assert.Equal(150m, result.TotalAmount);
        Assert.Single(result.Items);
        Assert.Equal(140m, result.Items[0].Amount);
        fixture.Realtime.Verify(r => r.SendInvoiceGeneratedAsync(fixture.CompletedOrderId, result.Id, fixture.ClientUserId), Times.Once);
    }

    [Fact]
    public async Task CreateInvoiceAsync_AlreadyInvoicedOrder_Throws()
    {
        var fixture = await SeedAsync();
        var request = new CreateInvoiceRequestDto
        {
            Orders = new() { new CreateInvoiceOrderItemDto { OrderId = fixture.AlreadyInvoicedOrderId, Price = 100m } }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.CreateInvoiceAsync(request, fixture.AdminUserId));

        Assert.Contains("already been invoiced", ex.Message);
    }

    [Fact]
    public async Task CreateInvoiceAsync_NegativeManualAmount_Throws()
    {
        var fixture = await SeedAsync();
        var request = new CreateInvoiceRequestDto
        {
            ManualItems = new()
            {
                new CreateInvoiceItemDto { Description = "bad", Amount = -1m, OrderId = fixture.CompletedOrderId }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.CreateInvoiceAsync(request, fixture.AdminUserId));
        Assert.Contains("cannot be negative", ex.Message);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_OverdueInvoiceDueDateMovedToFuture_ResetsToPending()
    {
        var fixture = await SeedAsync();
        var created = await fixture.Service.CreateInvoiceAsync(new CreateInvoiceRequestDto
        {
            Orders = new() { new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 111m } }
        }, fixture.AdminUserId);

        var invoice = await fixture.Context.Invoices.FirstAsync(i => i.Id == created.Id);
        invoice.Status = InvoiceStatus.Overdue;
        invoice.DueDate = DateTime.UtcNow.AddDays(-2);
        await fixture.Context.SaveChangesAsync();

        var updated = await fixture.Service.UpdateInvoiceAsync(created.Id, new UpdateInvoiceRequestDto
        {
            DueDate = DateTime.UtcNow.AddDays(15)
        }, fixture.AdminUserId);

        Assert.Equal("Pending", updated.Status);
    }

    [Fact]
    public async Task MarkInvoiceAsPaidAsync_AlreadyPaid_Throws()
    {
        var fixture = await SeedAsync();
        var created = await fixture.Service.CreateInvoiceAsync(new CreateInvoiceRequestDto
        {
            Orders = new() { new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 111m } }
        }, fixture.AdminUserId);
        await fixture.Service.MarkInvoiceAsPaidAsync(created.Id, "Cash", fixture.AdminUserId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.MarkInvoiceAsPaidAsync(created.Id, "Cash", fixture.AdminUserId));
        Assert.Contains("already marked as paid", ex.Message);
    }

    [Fact]
    public async Task UpdateOverdueInvoicesAsync_MarksOnlyUnpaidPastDueAndCreatesLogs()
    {
        var fixture = await SeedAsync();
        var created = await fixture.Service.CreateInvoiceAsync(new CreateInvoiceRequestDto
        {
            Orders = new() { new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 111m } }
        }, fixture.AdminUserId);

        var invoice = await fixture.Context.Invoices.FirstAsync(i => i.Id == created.Id);
        invoice.DueDate = DateTime.UtcNow.AddDays(-5);
        invoice.Status = InvoiceStatus.Pending;
        await fixture.Context.SaveChangesAsync();

        await fixture.Service.UpdateOverdueInvoicesAsync();

        var refreshed = await fixture.Context.Invoices.FirstAsync(i => i.Id == created.Id);
        Assert.Equal(InvoiceStatus.Overdue, refreshed.Status);
        Assert.True(await fixture.Context.InvoiceLogs.AnyAsync(l => l.InvoiceId == created.Id && l.Action == InvoiceAction.Overdue));
    }

    [Fact]
    public async Task SendInvoiceAsync_NotFound_ReturnsFalse()
    {
        var fixture = await SeedAsync();
        var sent = await fixture.Service.SendInvoiceAsync(Guid.NewGuid(), fixture.AdminUserId);
        Assert.False(sent);
    }

    [Fact]
    public async Task GetInvoiceStatisticsAsync_ClientRole_FiltersClientInvoicesOnly()
    {
        var fixture = await SeedAsync();
        await fixture.Service.CreateInvoiceAsync(new CreateInvoiceRequestDto
        {
            Orders = new() { new CreateInvoiceOrderItemDto { OrderId = fixture.CompletedOrderId, Price = 90m } }
        }, fixture.AdminUserId);

        var stats = await fixture.Service.GetInvoiceStatisticsAsync(fixture.ClientUserId, "Client");
        Assert.Equal(1, stats.TotalInvoices);
        Assert.Equal(90m, stats.TotalAmount);
    }

    private sealed record Fixture(
        ApplicationDbContext Context,
        InvoiceService Service,
        Mock<INotificationService> Notification,
        Mock<IRealtimeEntityUpdateSender> Realtime,
        Guid AdminUserId,
        Guid ClientUserId,
        Guid CompletedOrderId,
        Guid AlreadyInvoicedOrderId);

    private static async Task<Fixture> SeedAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("invoice-branches-" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new ApplicationDbContext(options);

        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var adminUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();

        var admin = new User { Id = adminUserId, Email = "admin@test.com", FirstName = "Admin", LastName = "User", PasswordHash = "h", RoleId = adminRole.Id, Role = adminRole };
        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "Client", LastName = "User", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, User = clientUser, CompanyName = "Co" };

        var completedOrderId = Guid.NewGuid();
        var alreadyInvoicedOrderId = Guid.NewGuid();
        var completed = new LogoOrder
        {
            Id = completedOrderId,
            ClientId = clientProfile.Id,
            Client = clientProfile,
            Status = OrderStatus.Completed,
            BillingEligible = true,
            IsInvoiced = false,
            Title = "Completed",
            Price = 90m,
            ClientChargePrice = 90m
        };
        var alreadyInvoiced = new LogoOrder
        {
            Id = alreadyInvoicedOrderId,
            ClientId = clientProfile.Id,
            Client = clientProfile,
            Status = OrderStatus.Completed,
            BillingEligible = true,
            IsInvoiced = true,
            Title = "Done",
            Price = 110m,
            ClientChargePrice = 110m
        };

        context.Roles.AddRange(adminRole, clientRole);
        context.Users.AddRange(admin, clientUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.AddRange(completed, alreadyInvoiced);
        await context.SaveChangesAsync();

        var notification = new Mock<INotificationService>();
        notification.Setup(n => n.CreateNotificationAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(),
            It.IsAny<Guid?>(), It.IsAny<NotificationReferenceType>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new NotificationResponseDto { Id = Guid.NewGuid() });
        notification.Setup(n => n.CreateNotificationForRoleAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(),
            It.IsAny<NotificationReferenceType>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new List<NotificationResponseDto>());

        var realtime = new Mock<IRealtimeEntityUpdateSender>();
        realtime.Setup(r => r.SendInvoiceGeneratedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var service = new InvoiceService(
            context,
            mapper,
            notification.Object,
            realtime.Object,
            Options.Create(new ProductionSafetyOptions()),
            Mock.Of<ILogger<InvoiceService>>());

        return new Fixture(context, service, notification, realtime, adminUserId, clientUserId, completedOrderId, alreadyInvoicedOrderId);
    }
}
