using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Exceptions;
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

public class PaymentServiceMutationAccessTests
{
    [Fact]
    public async Task CreatePaymentAsync_DesignerOnClientInvoice_ThrowsForbidden()
    {
        var (context, invoiceId, _, designerUserId) = await CreateClientInvoiceAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.CreatePaymentAsync(
                new CreatePaymentRequestDto
                {
                    InvoiceId = invoiceId,
                    PaymentMethod = "banktransfer",
                    Amount = 50m
                },
                designerUserId,
                "Designer"));

        await context.DisposeAsync();
    }

    [Fact]
    public async Task CreatePaymentAsync_OwningClient_Succeeds()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var service = CreateService(context);

        var result = await service.CreatePaymentAsync(
            new CreatePaymentRequestDto
            {
                InvoiceId = invoiceId,
                PaymentMethod = "banktransfer",
                Amount = 50m
            },
            clientUserId,
            "Client");

        Assert.NotEqual(Guid.Empty, result.Id);
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

    private static async Task<(ApplicationDbContext context, Guid invoiceId, Guid clientUserId, Guid designerUserId)>
        CreateClientInvoiceAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("PaymentMutation_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        context.Users.AddRange(
            new User { Id = clientUserId, Email = "c@test.com", PasswordHash = "h", RoleId = Guid.NewGuid() },
            new User { Id = designerUserId, Email = "d@test.com", PasswordHash = "h", RoleId = Guid.NewGuid() });

        var profile = new ClientProfile { Id = profileId, UserId = clientUserId, User = context.Users.Local.First(u => u.Id == clientUserId) };
        context.ClientProfiles.Add(profile);

        var invoiceId = Guid.NewGuid();
        context.Invoices.Add(new Invoice
        {
            Id = invoiceId,
            ClientId = profileId,
            InvoiceNumber = "INV-1",
            Amount = 50m,
            TotalAmount = 50m,
            Client = profile
        });
        await context.SaveChangesAsync();

        return (context, invoiceId, clientUserId, designerUserId);
    }
}
