using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Quotes;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class QuoteServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RespondToQuoteAsync_UpdatesQuoteAndStatus()
    {
        var context = CreateContext(nameof(RespondToQuoteAsync_UpdatesQuoteAndStatus));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "c@test.com", FirstName = "C", LastName = "L", PasswordHash = "x" };
        var client = new ClientProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user, CompanyName = "ACME" };
        var quote = new Quote { Id = Guid.NewGuid(), ClientId = client.Id, Client = client, LogoName = "Logo", Description = "Desc", Status = QuoteStatus.Pending };
        context.Roles.Add(role);
        context.Users.Add(user);
        context.ClientProfiles.Add(client);
        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(n => n.CreateNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Guid?>(),
                It.IsAny<NotificationReferenceType>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new NotificationResponseDto());
        var orderServiceMock = new Mock<IOrderService>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:Path"] = Path.GetTempPath()
        }).Build();

        var sut = new QuoteService(context, notificationMock.Object, orderServiceMock.Object, new ClientProfileEnsureService(context, Mock.Of<IReadModelCacheVersions>(), Mock.Of<Microsoft.Extensions.Logging.ILogger<ClientProfileEnsureService>>()), config);

        var result = await sut.RespondToQuoteAsync(quote.Id, new RespondQuoteRequestDto { AdminQuotedPrice = 120m, AdminNotes = "Ready" }, Guid.NewGuid());

        Assert.Equal("Responded", result.Status);
        Assert.Equal(120m, result.AdminQuotedPrice);
        Assert.Equal("Ready", result.AdminNotes);
    }

    [Fact]
    public async Task ConvertToOrderAsync_WithExistingOrder_LinksOrderAndMarksConverted()
    {
        var context = CreateContext(nameof(ConvertToOrderAsync_WithExistingOrder_LinksOrderAndMarksConverted));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "c2@test.com", FirstName = "C", LastName = "L", PasswordHash = "x" };
        var client = new ClientProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user, CompanyName = "ACME 2" };
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            LogoName = "Logo 2",
            Description = "Desc 2",
            Status = QuoteStatus.Responded,
            AdminQuotedPrice = 250m
        };
        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            Title = "Draft",
            Description = "Draft desc",
            Price = 0m,
            ClientBasePrice = 0m,
            ClientChargePrice = 0m
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        context.ClientProfiles.Add(client);
        context.Quotes.Add(quote);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(n => n.CreateNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Guid?>(),
                It.IsAny<NotificationReferenceType>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new NotificationResponseDto());
        var orderServiceMock = new Mock<IOrderService>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:Path"] = Path.GetTempPath()
        }).Build();
        var sut = new QuoteService(context, notificationMock.Object, orderServiceMock.Object, new ClientProfileEnsureService(context, Mock.Of<IReadModelCacheVersions>(), Mock.Of<Microsoft.Extensions.Logging.ILogger<ClientProfileEnsureService>>()), config);

        var result = await sut.ConvertToOrderAsync(quote.Id, user.Id, order.Id);
        var savedOrder = await context.LogoOrders.FirstAsync(o => o.Id == order.Id);

        Assert.Equal("Converted", result.Status);
        Assert.Equal(order.Id, result.ConvertedOrderId);
        Assert.Equal(OrderSource.Quote, savedOrder.OrderSource);
        Assert.Equal(quote.Id, savedOrder.QuoteId);
        Assert.Equal(250m, savedOrder.Price);
    }
}
