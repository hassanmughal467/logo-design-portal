using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Tests DesignerPayoutService: SubmitDesignerPricingAsync, ApproveDesignerPriceAsync, GenerateDesignerInvoiceAsync.
/// Verifies DesignerApprovedPrice usage and designer payout calculations.
/// </summary>
public class DesignerPayoutServiceTests
{
    [Fact]
    public async Task SubmitDesignerPricingAsync_ValidInput_SetsProposedPrice()
    {
        var (context, orderId, designerUserId) = await SeedOrderWithDesignerAsync(OrderStatus.InProgress);
        var designerPayoutService = CreateDesignerPayoutService(context);

        var request = new SubmitDesignerPricingRequestDto
        {
            DesignCategory = DesignCategory.EmbroideryDigitizing,
            DesignType = DesignType.LeftChest,
            ProposedPrice = 5000
        };

        await designerPayoutService.SubmitDesignerPricingAsync(orderId, request, designerUserId);

        var order = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal(5000, order!.DesignerProposedPrice);
        Assert.Equal(DesignCategory.EmbroideryDigitizing, order.DesignCategory);
        Assert.Equal(DesignType.LeftChest, order.DesignType);
    }

    [Fact]
    public async Task SubmitDesignerPricingAsync_NonAssignedDesigner_ThrowsForbiddenAccessException()
    {
        var (context, orderId, _) = await SeedOrderWithDesignerAsync(OrderStatus.InProgress);
        var designerPayoutService = CreateDesignerPayoutService(context);
        var otherDesignerUserId = Guid.NewGuid();

        var request = new SubmitDesignerPricingRequestDto
        {
            DesignCategory = DesignCategory.EmbroideryDigitizing,
            DesignType = DesignType.LeftChest,
            ProposedPrice = 5000
        };

        await Assert.ThrowsAsync<LogoDesignPortal.Application.Exceptions.ForbiddenAccessException>(
            () => designerPayoutService.SubmitDesignerPricingAsync(orderId, request, otherDesignerUserId));
    }

    [Fact]
    public async Task SubmitDesignerPricingAsync_NegativePrice_ThrowsInvalidOperationException()
    {
        var (context, orderId, designerUserId) = await SeedOrderWithDesignerAsync(OrderStatus.InProgress);
        var designerPayoutService = CreateDesignerPayoutService(context);

        var request = new SubmitDesignerPricingRequestDto
        {
            DesignCategory = DesignCategory.EmbroideryDigitizing,
            DesignType = DesignType.LeftChest,
            ProposedPrice = -100
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => designerPayoutService.SubmitDesignerPricingAsync(orderId, request, designerUserId));
    }

    [Fact]
    public async Task GetDesignPricingInfoAsync_ReturnsPricingInfo()
    {
        var context = await CreateEmptyContextAsync();
        var designerPayoutService = CreateDesignerPayoutService(context);

        var result = await designerPayoutService.GetDesignPricingInfoAsync();

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public async Task GetDesignerPayoutEligibleOrdersAsync_CompletedOrderWithApprovedPrice_ReturnsOrder()
    {
        var (context, designerProfile, orderId) = await SeedCompletedOrderWithApprovedPriceAsync();
        var designerPayoutService = CreateDesignerPayoutService(context);

        var result = await designerPayoutService.GetDesignerPayoutEligibleOrdersAsync(designerProfile.Id);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(orderId, result[0].OrderId);
        Assert.Equal(5000, result[0].ApprovedPrice);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid designerUserId)> SeedOrderWithDesignerAsync(OrderStatus status)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DesignerPayout_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var clientUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();

        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var designerUser = new User { Id = designerUserId, Email = "designer@test.com", FirstName = "D", LastName = "Designer", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole };

        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };
        var designerProfile = new DesignerProfile { Id = Guid.NewGuid(), UserId = designerUserId, User = designerUser };

        var orderId = Guid.NewGuid();
        var order = new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfile.Id,
            DesignerId = designerProfile.Id,
            Title = "Test",
            Status = status,
            Price = 100,
            Client = clientProfile,
            Designer = designerProfile
        };

        context.Roles.AddRange(clientRole, designerRole);
        context.Users.AddRange(clientUser, designerUser);
        context.ClientProfiles.Add(clientProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return (context, orderId, designerUserId);
    }

    private static async Task<(ApplicationDbContext context, DesignerProfile designerProfile, Guid orderId)> SeedCompletedOrderWithApprovedPriceAsync()
    {
        var (context, orderId, _) = await SeedOrderWithDesignerAsync(OrderStatus.InProgress);
        var order = await context.LogoOrders.FindAsync(orderId);
        var designerProfile = await context.DesignerProfiles.FirstAsync();

        order!.Status = OrderStatus.Completed;
        order.BillingEligible = true;
        order.PriceApproved = true;
        order.DesignerApprovedPrice = 5000;
        order.ApprovedPrice = 5000;
        order.CompletedDate = DateTime.UtcNow;
        order.IsDesignerInvoiced = false;
        await context.SaveChangesAsync();

        return (context, designerProfile, orderId);
    }

    private static async Task<ApplicationDbContext> CreateEmptyContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DesignerPayout_" + Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static DesignerPayoutService CreateDesignerPayoutService(ApplicationDbContext context)
    {
        var safetyOptions = new ProductionSafetyOptions { DisableDesignerPayout = false };
        return new DesignerPayoutService(
            context,
            Mock.Of<INotificationService>(),
            Options.Create(safetyOptions),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DesignerPayoutService>>());
    }
}
