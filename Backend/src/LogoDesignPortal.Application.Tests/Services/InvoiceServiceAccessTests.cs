using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces;
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

public class InvoiceServiceAccessTests
{
    [Fact]
    public async Task GetInvoiceByIdWithAccess_OtherClient_ReturnsNull()
    {
        var (context, ownerUserId, otherUserId, invoiceId) = await CreateTwoClientsWithInvoiceAsync();
        var service = CreateService(context);

        var result = await service.GetInvoiceByIdWithAccessAsync(invoiceId, otherUserId, "Client");

        Assert.Null(result);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetInvoiceByIdWithAccess_OwnerClient_ReturnsInvoice()
    {
        var (context, ownerUserId, _, invoiceId) = await CreateTwoClientsWithInvoiceAsync();
        var service = CreateService(context);

        var result = await service.GetInvoiceByIdWithAccessAsync(invoiceId, ownerUserId, "Client");

        Assert.NotNull(result);
        Assert.Equal(invoiceId, result!.Id);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetInvoiceByIdWithAccess_Designer_ReturnsNull()
    {
        var (context, ownerUserId, _, invoiceId) = await CreateTwoClientsWithInvoiceAsync();
        var service = CreateService(context);

        var result = await service.GetInvoiceByIdWithAccessAsync(invoiceId, Guid.NewGuid(), "Designer");

        Assert.Null(result);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetInvoices_Designer_ReturnsEmpty()
    {
        var (context, ownerUserId, _, _) = await CreateTwoClientsWithInvoiceAsync();
        var service = CreateService(context);

        var page = await service.GetInvoicesAsync(Guid.NewGuid(), "Designer");

        Assert.Empty(page.Items);
        Assert.Equal(0, page.Total);
        await context.DisposeAsync();
    }

    private static InvoiceService CreateService(ApplicationDbContext context)
    {
        return new InvoiceService(
            context,
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Options.Create(new ProductionSafetyOptions()),
            Mock.Of<IReadModelCacheVersions>(),
            Mock.Of<ILogger<InvoiceService>>());
    }

    private static async Task<(ApplicationDbContext context, Guid ownerUserId, Guid otherUserId, Guid invoiceId)>
        CreateTwoClientsWithInvoiceAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("InvoiceAccess_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var ownerProfileId = Guid.NewGuid();

        var ownerUser = new User
        {
            Id = ownerUserId,
            Email = "owner@test.com",
            FirstName = "Owner",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };
        var otherUser = new User
        {
            Id = otherUserId,
            Email = "other@test.com",
            FirstName = "Other",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };

        context.Roles.Add(clientRole);
        context.Users.AddRange(ownerUser, otherUser);
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = ownerProfileId,
            UserId = ownerUserId,
            CompanyName = "Owner Co",
            User = ownerUser
        });

        var invoiceId = Guid.NewGuid();
        context.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            ClientId = ownerProfileId,
            InvoiceNumber = "INV-001",
            Amount = 100m,
            TaxAmount = 0m,
            TotalAmount = 100m,
            Status = InvoiceStatus.Pending,
            BillingType = BillingType.PerLogo,
            Client = context.ClientProfiles.Local.First()
        });

        await context.SaveChangesAsync();
        return (context, ownerUserId, otherUserId, invoiceId);
    }
}
