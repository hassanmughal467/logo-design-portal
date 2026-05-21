using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class PaymentServiceAccessTests
{
    [Fact]
    public async Task GetPaymentByIdWithAccess_OtherClient_ReturnsNull()
    {
        var (context, _, otherUserId, paymentId, _) = await CreatePaymentForOwnerAsync();
        var service = CreateService(context);

        var result = await service.GetPaymentByIdWithAccessAsync(paymentId, otherUserId, "Client");

        Assert.Null(result);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetPaymentsByInvoiceWithAccess_OtherClient_ReturnsNull()
    {
        var created = await CreatePaymentForOwnerAsync();
        var (context, _, otherUserId, invoiceId) = (created.context, created.ownerUserId, created.otherUserId, created.invoiceId);
        var service = CreateService(context);

        var result = await service.GetPaymentsByInvoiceWithAccessAsync(invoiceId, otherUserId, "Client");

        Assert.Null(result);
        await context.DisposeAsync();
    }

    private static PaymentService CreateService(ApplicationDbContext context)
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return new PaymentService(
            context,
            mapperConfig.CreateMapper(),
            Mock.Of<LogoDesignPortal.Application.Interfaces.ISettingsService>(),
            Mock.Of<LogoDesignPortal.Application.Interfaces.INotificationService>(),
            Mock.Of<IReadModelCacheVersions>(),
            Mock.Of<ILogger<PaymentService>>(),
            Mock.Of<IHttpClientFactory>());
    }

    private static async Task<(ApplicationDbContext context, Guid ownerUserId, Guid otherUserId, Guid paymentId, Guid invoiceId)>
        CreatePaymentForOwnerAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("PaymentAccess_" + Guid.NewGuid())
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
        var profile = new ClientProfile
        {
            Id = ownerProfileId,
            UserId = ownerUserId,
            CompanyName = "Owner Co",
            User = ownerUser
        };
        context.ClientProfiles.Add(profile);

        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            ClientId = ownerProfileId,
            InvoiceNumber = "INV-PAY-1",
            Amount = 50m,
            TaxAmount = 0m,
            TotalAmount = 50m,
            Status = InvoiceStatus.Pending,
            Client = profile
        };
        context.Invoices.Add(invoice);

        var paymentId = Guid.NewGuid();
        context.Payments.Add(new Payment
        {
            Id = paymentId,
            InvoiceId = invoiceId,
            PaymentMethod = "banktransfer",
            Amount = 50m,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            Invoice = invoice
        });

        await context.SaveChangesAsync();
        return (context, ownerUserId, otherUserId, paymentId, invoiceId);
    }
}
