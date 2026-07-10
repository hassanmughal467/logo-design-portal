using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Constants;
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
                designerUserId));

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
            clientUserId);

        Assert.NotEqual(Guid.Empty, result.Id);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task CreatePaymentAsync_OtherClient_ThrowsForbidden()
    {
        var (context, invoiceId, _, _) = await CreateClientInvoiceAsync();
        var otherClientUserId = await SeedOtherClientAsync(context);
        var service = CreateService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.CreatePaymentAsync(
                new CreatePaymentRequestDto
                {
                    InvoiceId = invoiceId,
                    PaymentMethod = "banktransfer",
                    Amount = 50m
                },
                otherClientUserId));

        Assert.Empty(await context.Payments.ToListAsync());
        await context.DisposeAsync();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    public async Task CreatePaymentAsync_AdminRoles_Succeed(string role)
    {
        var (context, invoiceId, _, _) = await CreateClientInvoiceAsync();
        var userId = await SeedUserWithRoleAsync(context, role);
        var service = CreateService(context);

        var result = await service.CreatePaymentAsync(
            new CreatePaymentRequestDto
            {
                InvoiceId = invoiceId,
                PaymentMethod = "banktransfer",
                Amount = 50m
            },
            userId);

        Assert.NotEqual(Guid.Empty, result.Id);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GeneratePaymentLinkAsync_DesignerOnClientInvoice_ThrowsForbidden()
    {
        var (context, invoiceId, _, designerUserId) = await CreateClientInvoiceAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.GeneratePaymentLinkAsync(invoiceId, "banktransfer", designerUserId));

        await context.DisposeAsync();
    }

    [Fact]
    public async Task GeneratePaymentLinkAsync_OwningClient_Succeeds()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var service = CreateService(context);

        var result = await service.GeneratePaymentLinkAsync(invoiceId, "banktransfer", clientUserId);

        Assert.Equal(invoiceId, result.InvoiceId);
        Assert.NotEqual(Guid.Empty, result.PaymentId);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GeneratePaymentLinkAsync_OtherClient_ThrowsForbidden()
    {
        var (context, invoiceId, _, _) = await CreateClientInvoiceAsync();
        var otherClientUserId = await SeedOtherClientAsync(context);
        var service = CreateService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.GeneratePaymentLinkAsync(invoiceId, "banktransfer", otherClientUserId));

        await context.DisposeAsync();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    public async Task GeneratePaymentLinkAsync_AdminRoles_Succeed(string role)
    {
        var (context, invoiceId, _, _) = await CreateClientInvoiceAsync();
        var userId = await SeedUserWithRoleAsync(context, role);
        var service = CreateService(context);

        var result = await service.GeneratePaymentLinkAsync(invoiceId, "banktransfer", userId);

        Assert.Equal(invoiceId, result.InvoiceId);
        Assert.NotEqual(Guid.Empty, result.PaymentId);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task ProcessPaymentAsync_OwningClientBankTransferReference_SetsProcessingAndLeavesInvoicePending()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var paymentId = await SeedPendingBankTransferPaymentAsync(context, invoiceId, clientUserId);
        var service = CreateService(context);

        var result = await service.ProcessPaymentAsync(
            new ProcessPaymentRequestDto
            {
                PaymentId = paymentId,
                BankReference = " BT-12345 "
            },
            clientUserId);

        Assert.Equal(PaymentStatus.Processing.ToString(), result.Status);
        Assert.Equal("BT-12345", result.TransactionId);
        Assert.Null(result.CompletedAt);

        var payment = await context.Payments.Include(p => p.Invoice).SingleAsync(p => p.Id == paymentId);
        Assert.Equal(PaymentStatus.Processing, payment.Status);
        Assert.Equal(InvoiceStatus.Pending, payment.Invoice.Status);
        Assert.Null(payment.Invoice.PaidDate);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task ProcessPaymentAsync_BankTransferWithoutReference_ThrowsInvalidOperationException()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var paymentId = await SeedPendingBankTransferPaymentAsync(context, invoiceId, clientUserId);
        var service = CreateService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProcessPaymentAsync(
                new ProcessPaymentRequestDto
                {
                    PaymentId = paymentId,
                    BankReference = " "
                },
                clientUserId));

        Assert.Contains("Bank reference is required", ex.Message);
        var payment = await context.Payments.Include(p => p.Invoice).SingleAsync(p => p.Id == paymentId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(InvoiceStatus.Pending, payment.Invoice.Status);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task ProcessPaymentAsync_OtherClientBankTransfer_ThrowsForbidden()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var otherClientUserId = await SeedOtherClientAsync(context);
        var paymentId = await SeedPendingBankTransferPaymentAsync(context, invoiceId, clientUserId);
        var service = CreateService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.ProcessPaymentAsync(
                new ProcessPaymentRequestDto
                {
                    PaymentId = paymentId,
                    BankReference = "BT-OTHER"
                },
                otherClientUserId));

        var payment = await context.Payments.Include(p => p.Invoice).SingleAsync(p => p.Id == paymentId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(InvoiceStatus.Pending, payment.Invoice.Status);
        await context.DisposeAsync();
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_AdminVerification_CompletesPaymentAndMarksInvoicePaid()
    {
        var (context, invoiceId, clientUserId, _) = await CreateClientInvoiceAsync();
        var paymentId = await SeedPendingBankTransferPaymentAsync(context, invoiceId, clientUserId);
        var service = CreateService(context);

        var result = await service.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Completed.ToString(), "BT-VERIFIED");

        Assert.Equal(PaymentStatus.Completed.ToString(), result.Status);
        Assert.Equal("BT-VERIFIED", result.TransactionId);
        var payment = await context.Payments.Include(p => p.Invoice).SingleAsync(p => p.Id == paymentId);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal(InvoiceStatus.Paid, payment.Invoice.Status);
        Assert.NotNull(payment.Invoice.PaidDate);
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

        SeedRoles(context);

        var clientRole = context.Roles.Local.First(r => r.Name == nameof(SystemRoles.Client));
        var designerRole = context.Roles.Local.First(r => r.Name == nameof(SystemRoles.Designer));
        context.Users.AddRange(
            new User { Id = clientUserId, Email = "c@test.com", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole },
            new User { Id = designerUserId, Email = "d@test.com", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole });

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

    private static async Task<Guid> SeedPendingBankTransferPaymentAsync(ApplicationDbContext context, Guid invoiceId, Guid createdBy)
    {
        var paymentId = Guid.NewGuid();
        context.Payments.Add(new Payment
        {
            Id = paymentId,
            InvoiceId = invoiceId,
            PaymentMethod = "banktransfer",
            Amount = 50m,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        return paymentId;
    }

    private static async Task<Guid> SeedOtherClientAsync(ApplicationDbContext context)
    {
        var otherClientUserId = Guid.NewGuid();
        var otherProfileId = Guid.NewGuid();
        var clientRole = await context.Roles.SingleAsync(r => r.Name == nameof(SystemRoles.Client));
        var otherUser = new User
        {
            Id = otherClientUserId,
            Email = "other-client@test.com",
            PasswordHash = "h",
            RoleId = clientRole.Id,
            Role = clientRole,
            IsActive = true
        };

        context.Users.Add(otherUser);
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = otherProfileId,
            UserId = otherClientUserId,
            User = otherUser,
            CompanyName = "Other Client"
        });
        await context.SaveChangesAsync();
        return otherClientUserId;
    }

    private static async Task<Guid> SeedUserWithRoleAsync(ApplicationDbContext context, string roleName)
    {
        var role = await context.Roles.SingleAsync(r => r.Name == roleName);
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = $"{roleName.ToLowerInvariant()}-{Guid.NewGuid():N}@test.com",
            PasswordHash = "h",
            RoleId = role.Id,
            Role = role,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return userId;
    }

    private static void SeedRoles(ApplicationDbContext context)
    {
        context.Roles.AddRange(
            new Role { Id = SeededRoleIds.SuperAdmin, Name = nameof(SystemRoles.SuperAdmin) },
            new Role { Id = SeededRoleIds.Admin, Name = nameof(SystemRoles.Admin) },
            new Role { Id = SeededRoleIds.Designer, Name = nameof(SystemRoles.Designer) },
            new Role { Id = SeededRoleIds.Client, Name = nameof(SystemRoles.Client) });
    }
}
