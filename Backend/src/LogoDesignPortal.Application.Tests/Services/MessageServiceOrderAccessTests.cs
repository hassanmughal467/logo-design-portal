using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// TEST-002: Verifies MessageService order access - Client/Designer can only send messages for orders they have access to.
/// </summary>
public class MessageServiceOrderAccessTests
{
    [Fact]
    public async Task CreateMessage_ClientSendsForAnotherClientsOrder_ThrowsForbiddenAccessException()
    {
        var data = await SeedAndGetContextAsync();
        var messageService = CreateMessageService(data.Context);
        var request = new CreateMessageRequestDto { OrderId = data.OrderBId, Content = "Test message" };

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => messageService.CreateMessageAsync(request, data.ClientAUserId));

        Assert.Contains("don't have access", ex.Message);
    }

    [Fact]
    public async Task CreateMessage_DesignerSendsForOrderNotAssignedToThem_ThrowsForbiddenAccessException()
    {
        var data = await SeedAndGetContextAsync();
        var messageService = CreateMessageService(data.Context);
        var request = new CreateMessageRequestDto { OrderId = data.OrderBId, Content = "Test message" };

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => messageService.CreateMessageAsync(request, data.DesignerUserId));

        Assert.Contains("don't have access", ex.Message);
    }

    [Fact]
    public async Task CreateMessage_ClientSendsForOwnOrder_Succeeds()
    {
        var data = await SeedAndGetContextAsync();
        var messageService = CreateMessageService(data.Context);
        var request = new CreateMessageRequestDto { OrderId = data.OrderAId, Content = "Test message" };

        var result = await messageService.CreateMessageAsync(request, data.ClientAUserId);

        Assert.NotNull(result);
        Assert.Equal(data.OrderAId, result.OrderId);
        Assert.Equal("Test message", result.Content);
    }

    private static async Task<(ApplicationDbContext Context, Guid OrderAId, Guid OrderBId, Guid ClientAUserId, Guid DesignerUserId)> SeedAndGetContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MessageOrder_" + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };

        var clientAUserId = Guid.NewGuid();
        var clientBUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();

        var clientA = new User { Id = clientAUserId, Email = "clientA@test.com", FirstName = "A", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientB = new User { Id = clientBUserId, Email = "clientB@test.com", FirstName = "B", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var designer = new User { Id = designerUserId, Email = "designer@test.com", FirstName = "D", LastName = "Designer", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole };

        var clientAProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientAUserId, CompanyName = "Client A", User = clientA };
        var clientBProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientBUserId, CompanyName = "Client B", User = clientB };
        var designerProfile = new DesignerProfile { Id = Guid.NewGuid(), UserId = designerUserId, User = designer };

        var orderAId = Guid.NewGuid();
        var orderBId = Guid.NewGuid();

        var orderA = new LogoOrder
        {
            Id = orderAId,
            ClientId = clientAProfile.Id,
            DesignerId = designerProfile.Id,
            Title = "Order A",
            Status = OrderStatus.InProgress,
            Price = 100,
            Client = clientAProfile,
            Designer = designerProfile
        };

        var orderB = new LogoOrder
        {
            Id = orderBId,
            ClientId = clientBProfile.Id,
            DesignerId = null,
            Title = "Order B",
            Status = OrderStatus.InProgress,
            Price = 100,
            Client = clientBProfile,
            Designer = null
        };

        context.Roles.AddRange(clientRole, designerRole);
        context.Users.AddRange(clientA, clientB, designer);
        context.ClientProfiles.AddRange(clientAProfile, clientBProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.AddRange(orderA, orderB);
        await context.SaveChangesAsync();

        return (context, orderAId, orderBId, clientAUserId, designerUserId);
    }

    private static MessageService CreateMessageService(ApplicationDbContext context)
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<LogoDesignPortal.Application.Mappings.MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        return new MessageService(
            context,
            mapper,
            Mock.Of<INotificationService>());
    }
}
