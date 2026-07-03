using System.Text;
using System.Text.Json;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Quotes;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Application.Tests.Storage;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
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
        var storageRoot = Path.GetTempPath();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:Path"] = storageRoot
        }).Build();

        var sut = CreateQuoteService(context, storageRoot, notificationMock.Object, orderServiceMock.Object);

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
        var storageRoot = Path.GetTempPath();
        var sut = CreateQuoteService(context, storageRoot, notificationMock.Object, orderServiceMock.Object);

        var result = await sut.ConvertToOrderAsync(quote.Id, user.Id, order.Id);
        var savedOrder = await context.LogoOrders.FirstAsync(o => o.Id == order.Id);

        Assert.Equal("Converted", result.Status);
        Assert.Equal(order.Id, result.ConvertedOrderId);
        Assert.Equal(OrderSource.Quote, savedOrder.OrderSource);
        Assert.Equal(quote.Id, savedOrder.QuoteId);
        Assert.Equal(250m, savedOrder.Price);
    }

    [Fact]
    public async Task CreateQuoteAsync_UsesClientProfileCurrency()
    {
        var context = CreateContext(nameof(CreateQuoteAsync_UsesClientProfileCurrency));
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            RoleId = clientRole.Id,
            Role = clientRole,
            Email = "gbp@test.com",
            FirstName = "G",
            LastName = "B",
            PasswordHash = "x"
        };
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CompanyName = "GBP Co",
            CurrencyCode = "GBP"
        };
        context.Roles.Add(clientRole);
        context.Users.Add(user);
        context.ClientProfiles.Add(client);
        await context.SaveChangesAsync();

        var storageRoot = Path.GetTempPath();
        var sut = CreateQuoteService(context, storageRoot, Mock.Of<INotificationService>(), Mock.Of<IOrderService>());

        var result = await sut.CreateQuoteAsync(
            new CreateQuoteRequestDto { LogoName = "Logo GBP", Description = "Test", RequestedBudget = 10m },
            Array.Empty<IFormFile>(),
            user.Id);

        Assert.Equal("GBP", result.CurrencyCode);
    }

    [Fact]
    public async Task RejectQuoteAsync_NotifiesAdminAndSuperAdmin()
    {
        var context = CreateContext(nameof(RejectQuoteAsync_NotifiesAdminAndSuperAdmin));
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            RoleId = clientRole.Id,
            Role = clientRole,
            Email = "reject@test.com",
            FirstName = "Kelvin",
            LastName = "Walter",
            PasswordHash = "x"
        };
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CompanyName = "Kelvin Co"
        };
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            LogoName = "Testing quote 1",
            Description = "Desc",
            Status = QuoteStatus.Responded,
            AdminQuotedPrice = 4m
        };
        context.Roles.Add(clientRole);
        context.Users.Add(user);
        context.ClientProfiles.Add(client);
        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(n => n.CreateNotificationForRoleAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<NotificationReferenceType>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new List<NotificationResponseDto>());

        var storageRoot = Path.GetTempPath();
        var sut = CreateQuoteService(context, storageRoot, notificationMock.Object, Mock.Of<IOrderService>());

        var result = await sut.RejectQuoteAsync(quote.Id, user.Id);

        Assert.Equal("Rejected", result.Status);
        notificationMock.Verify(
            n => n.CreateNotificationForRoleAsync(
                "Admin",
                "Quote Declined",
                It.Is<string>(m => m.Contains("Testing quote 1") && m.Contains("Kelvin Co")),
                NotificationType.Warning,
                NotificationReferenceType.Quote,
                quote.Id,
                user.Id),
            Times.Once);
        notificationMock.Verify(
            n => n.CreateNotificationForRoleAsync(
                "SuperAdmin",
                "Quote Declined",
                It.Is<string>(m => m.Contains("Testing quote 1") && m.Contains("Kelvin Co")),
                NotificationType.Warning,
                NotificationReferenceType.Quote,
                quote.Id,
                user.Id),
            Times.Once);
    }

    [Fact]
    public async Task DownloadQuoteAttachmentAsync_AsSuperAdmin_ReturnsFileBytes()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "quote-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(storageRoot, "Quotes"));
        var storedName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(storageRoot, "Quotes", storedName);
        await File.WriteAllBytesAsync(filePath, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var context = CreateContext(nameof(DownloadQuoteAttachmentAsync_AsSuperAdmin_ReturnsFileBytes));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "c3@test.com", FirstName = "C", LastName = "L", PasswordHash = "x" };
        var client = new ClientProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user, CompanyName = "ACME 3" };
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            LogoName = "Logo 3",
            Description = "Desc 3",
            Status = QuoteStatus.Pending,
            AttachmentsJson = JsonSerializer.Serialize(new List<string> { storedName })
        };
        context.Roles.Add(role);
        context.Users.Add(user);
        context.ClientProfiles.Add(client);
        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var sut = CreateQuoteService(context, storageRoot, Mock.Of<INotificationService>(), Mock.Of<IOrderService>());

        var adminId = Guid.NewGuid();
        var (content, fileName, contentType) = await sut.DownloadQuoteAttachmentAsync(quote.Id, storedName, adminId, "SuperAdmin");

        Assert.NotEmpty(content);
        Assert.Equal(storedName, fileName);
        Assert.Equal("image/png", contentType);

        try { Directory.Delete(storageRoot, true); } catch { /* best effort */ }
    }

    private static QuoteService CreateQuoteService(
        ApplicationDbContext context,
        string storageRoot,
        INotificationService notificationService,
        IOrderService orderService)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:Path"] = storageRoot
        }).Build();

        return new QuoteService(
            context,
            notificationService,
            orderService,
            new ClientProfileEnsureService(context, Mock.Of<IReadModelCacheVersions>(), Mock.Of<Microsoft.Extensions.Logging.ILogger<ClientProfileEnsureService>>()),
            TestFileStorageFactory.CreateLocal(storageRoot),
            config,
            TestFileStorageFactory.CreateLocalOptions(storageRoot));
    }
}
