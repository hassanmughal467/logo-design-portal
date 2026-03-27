using AutoMapper;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Exceptions;
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

public class MessageServiceTests
{
    [Fact]
    public async Task CreateMessageAsync_AdminToClient_SucceedsAndNotifiesRecipient()
    {
        var (context, senderId, recipientId, _, _, _) = await SeedContextAsync();
        var notification = new Mock<INotificationService>();
        var service = CreateService(context, notification.Object);

        var dto = await service.CreateMessageAsync(new CreateMessageRequestDto
        {
            RecipientId = recipientId,
            Content = "Please review invoice updates.",
        }, senderId);

        Assert.Equal(senderId, dto.SenderId);
        Assert.Equal(recipientId, dto.RecipientId);
        Assert.False(dto.RequiresAdminApproval);
        notification.Verify(n => n.CreateNotificationAsync(
            recipientId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.Info,
            It.IsAny<Guid?>(),
            NotificationReferenceType.Message,
            It.IsAny<Guid?>(),
            senderId), Times.Once);
    }

    [Fact]
    public async Task CreateMessageAsync_EmptyContent_Throws()
    {
        var (context, senderId, recipientId, _, _, _) = await SeedContextAsync();
        var service = CreateService(context, Mock.Of<INotificationService>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMessageAsync(new CreateMessageRequestDto
            {
                RecipientId = recipientId,
                Content = "   "
            }, senderId));

        Assert.Equal("Message content cannot be empty.", ex.Message);
    }

    [Fact]
    public async Task CreateMessageAsync_InvalidRecipient_Throws()
    {
        var (context, senderId, _, _, _, _) = await SeedContextAsync();
        var service = CreateService(context, Mock.Of<INotificationService>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMessageAsync(new CreateMessageRequestDto
            {
                RecipientId = Guid.NewGuid(),
                Content = "hello"
            }, senderId));

        Assert.Equal("Recipient not found.", ex.Message);
    }

    [Fact]
    public async Task CreateMessageAsync_ClientToDesigner_ThrowsInvalidOperation()
    {
        var (context, _, _, clientId, designerId, _) = await SeedContextAsync();
        var service = CreateService(context, Mock.Of<INotificationService>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMessageAsync(new CreateMessageRequestDto
            {
                RecipientId = designerId,
                Content = "Can we talk directly?"
            }, clientId));

        Assert.Contains("cannot message designers directly", ex.Message);
    }

    [Fact]
    public async Task CreateMessageAsync_NotificationFailure_DoesNotFailMessageCreation()
    {
        var (context, senderId, recipientId, _, _, _) = await SeedContextAsync();
        var notification = new Mock<INotificationService>();
        notification.Setup(n => n.CreateNotificationAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(),
                It.IsAny<Guid?>(), It.IsAny<NotificationReferenceType>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("notification transport failure"));
        var service = CreateService(context, notification.Object);

        var result = await service.CreateMessageAsync(new CreateMessageRequestDto
        {
            RecipientId = recipientId,
            Content = "This should still be persisted."
        }, senderId);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(1, await context.Messages.CountAsync());
    }

    [Fact]
    public async Task CreateMessageAsync_DesignerWithoutOrderAccess_ThrowsForbidden()
    {
        var (context, _, _, _, designerId, inaccessibleOrderId) = await SeedContextAsync();
        var service = CreateService(context, Mock.Of<INotificationService>());

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.CreateMessageAsync(new CreateMessageRequestDto
            {
                OrderId = inaccessibleOrderId,
                Content = "attempt over unrelated order"
            }, designerId));

        Assert.Contains("don't have access", ex.Message);
    }

    private static MessageService CreateService(ApplicationDbContext context, INotificationService notification) =>
        new(context, CreateMapper(), notification);

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static async Task<(ApplicationDbContext context, Guid adminId, Guid clientRecipientId, Guid clientId, Guid designerId, Guid inaccessibleOrderId)> SeedContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("msg-service-tests-" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };

        var adminId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientRecipientId = Guid.NewGuid();
        var designerId = Guid.NewGuid();

        var admin = new User { Id = adminId, RoleId = adminRole.Id, Role = adminRole, Email = "admin@test.com", PasswordHash = "h", FirstName = "Admin", LastName = "User" };
        var client = new User { Id = clientId, RoleId = clientRole.Id, Role = clientRole, Email = "client@test.com", PasswordHash = "h", FirstName = "Client", LastName = "A" };
        var clientRecipient = new User { Id = clientRecipientId, RoleId = clientRole.Id, Role = clientRole, Email = "client2@test.com", PasswordHash = "h", FirstName = "Client", LastName = "B" };
        var designer = new User { Id = designerId, RoleId = designerRole.Id, Role = designerRole, Email = "designer@test.com", PasswordHash = "h", FirstName = "Designer", LastName = "A" };

        var clientProfileA = new ClientProfile { Id = Guid.NewGuid(), UserId = clientId, User = client, CompanyName = "A" };
        var clientProfileB = new ClientProfile { Id = Guid.NewGuid(), UserId = clientRecipientId, User = clientRecipient, CompanyName = "B" };
        var designerProfile = new DesignerProfile { Id = Guid.NewGuid(), UserId = designerId, User = designer };

        var accessibleOrder = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfileA.Id,
            Client = clientProfileA,
            DesignerId = designerProfile.Id,
            Designer = designerProfile,
            Status = OrderStatus.InProgress,
            Title = "Order",
            Price = 100
        };
        var inaccessibleOrderId = Guid.NewGuid();
        var inaccessibleOrder = new LogoOrder
        {
            Id = inaccessibleOrderId,
            ClientId = clientProfileB.Id,
            Client = clientProfileB,
            Status = OrderStatus.InProgress,
            Title = "Order B",
            Price = 120
        };

        context.Roles.AddRange(adminRole, clientRole, designerRole);
        context.Users.AddRange(admin, client, clientRecipient, designer);
        context.ClientProfiles.AddRange(clientProfileA, clientProfileB);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.AddRange(accessibleOrder, inaccessibleOrder);
        await context.SaveChangesAsync();

        return (context, adminId, clientRecipientId, clientId, designerId, inaccessibleOrderId);
    }
}
