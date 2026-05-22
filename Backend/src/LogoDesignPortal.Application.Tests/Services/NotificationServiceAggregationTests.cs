using AutoMapper;
using LogoDesignPortal.Application.BackgroundJobs;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class NotificationServiceAggregationTests
{
    [Fact]
    public async Task CreateNotificationAsync_DifferentTitles_SameOrder_DoNotAggregate()
    {
        var (context, userId, orderId) = await SeedUserAndOrderAsync();
        var service = CreateService(context);
        var orderRef = orderId;

        await service.CreateNotificationAsync(
            userId,
            "Client Approved Logo",
            "Client approved the logo for order (#ORD-TEST). Admin will finalize completion.",
            NotificationType.OrderStatusChange,
            orderRef,
            NotificationReferenceType.Order,
            orderRef);

        await service.CreateNotificationAsync(
            userId,
            "Order Completed",
            "Order (#ORD-TEST) has been completed",
            NotificationType.OrderStatusChange,
            orderRef,
            NotificationReferenceType.Order,
            orderRef);

        var notifications = await context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        Assert.Equal(2, notifications.Count);
        Assert.All(notifications, n => Assert.Equal(1, n.AggregationCount));
        Assert.Contains(notifications, n => n.Title == "Client Approved Logo");
        Assert.Contains(notifications, n => n.Title == "Order Completed" && n.Message == "Order (#ORD-TEST) has been completed");
    }

    [Fact]
    public async Task CreateNotificationAsync_SameTitle_SameOrder_AggregatesWithinWindow()
    {
        var (context, userId, orderId) = await SeedUserAndOrderAsync();
        var service = CreateService(context);
        var orderRef = orderId;

        await service.CreateNotificationAsync(
            userId,
            "Order Completed",
            "Order (#ORD-TEST) has been completed",
            NotificationType.OrderStatusChange,
            orderRef,
            NotificationReferenceType.Order,
            orderRef);

        await service.CreateNotificationAsync(
            userId,
            "Order Completed",
            "Order (#ORD-TEST) has been completed",
            NotificationType.OrderStatusChange,
            orderRef,
            NotificationReferenceType.Order,
            orderRef);

        var notifications = await context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        Assert.Single(notifications);
        Assert.Equal(2, notifications[0].AggregationCount);
        Assert.Equal("2 orders (#ORD-TEST) has been completed", notifications[0].Message);
    }

    private static NotificationService CreateService(ApplicationDbContext context)
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var settings = new Mock<ISettingsService>();
        var realtime = new Mock<IRealtimeNotificationSender>();
        realtime
            .Setup(r => r.SendNotificationToUserAsync(It.IsAny<Guid>(), It.IsAny<Application.DTOs.Notifications.NotificationResponseDto>()))
            .Returns(Task.CompletedTask);
        var config = new Mock<IConfiguration>();
        var jobs = new Mock<IBackgroundJobScheduler>();

        return new NotificationService(context, mapper, settings.Object, realtime.Object, config.Object, jobs.Object);
    }

    private static async Task<(ApplicationDbContext context, Guid userId, Guid orderId)> SeedUserAndOrderAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("NotificationAgg_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var role = new Role { Id = Guid.NewGuid(), Name = "Designer", CreatedAt = DateTime.UtcNow };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "designer@test.com",
            FirstName = "Test",
            LastName = "Designer",
            RoleId = role.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            Title = "Test Order",
            Status = OrderStatus.ClientApproved,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return (context, user.Id, order.Id);
    }
}
