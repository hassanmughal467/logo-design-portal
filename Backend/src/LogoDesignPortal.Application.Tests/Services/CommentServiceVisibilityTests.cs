using AutoMapper;
using LogoDesignPortal.Application.DTOs.Comments;
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

public class CommentServiceVisibilityTests
{
    private static (ApplicationDbContext Context, CommentService Sut) CreateSut(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var notifications = Mock.Of<INotificationService>();
        var sut = new CommentService(context, mapper, notifications);
        return (context, sut);
    }

    private static async Task<(Guid OrderId, Guid SuperAdminId, Guid DesignerUserId, Guid ClientUserId)> SeedOrderWithPartiesAsync(
        ApplicationDbContext context)
    {
        var superAdminRoleId = Guid.NewGuid();
        var designerRoleId = Guid.NewGuid();
        var clientRoleId = Guid.NewGuid();

        context.Roles.AddRange(
            new Role { Id = superAdminRoleId, Name = "SuperAdmin", CreatedAt = DateTime.UtcNow },
            new Role { Id = designerRoleId, Name = "Designer", CreatedAt = DateTime.UtcNow },
            new Role { Id = clientRoleId, Name = "Client", CreatedAt = DateTime.UtcNow });

        var superAdminId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();

        context.Users.AddRange(
            new User
            {
                Id = superAdminId,
                Email = "sa@test.com",
                FirstName = "Super",
                LastName = "Admin",
                PasswordHash = "x",
                RoleId = superAdminRoleId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = designerUserId,
                Email = "designer@test.com",
                FirstName = "Des",
                LastName = "Igner",
                PasswordHash = "x",
                RoleId = designerRoleId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = clientUserId,
                Email = "client@test.com",
                FirstName = "Cli",
                LastName = "Ent",
                PasswordHash = "x",
                RoleId = clientRoleId,
                CreatedAt = DateTime.UtcNow
            });

        var clientProfileId = Guid.NewGuid();
        var designerProfileId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientProfileId,
            UserId = clientUserId,
            CompanyName = "Co",
            CreatedAt = DateTime.UtcNow
        });
        context.DesignerProfiles.Add(new DesignerProfile
        {
            Id = designerProfileId,
            UserId = designerUserId,
            Specialization = "Logo",
            CreatedAt = DateTime.UtcNow
        });

        var orderId = Guid.NewGuid();
        context.LogoOrders.Add(new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfileId,
            DesignerId = designerProfileId,
            Title = "Test",
            Description = "d",
            Status = OrderStatus.InProgress,
            Price = 100m,
            ClientBasePrice = 100m,
            ClientChargePrice = 100m,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return (orderId, superAdminId, designerUserId, clientUserId);
    }

    [Fact]
    public async Task CreateCommentAsync_SuperAdminNonInternal_UsesAdminRelay_AndHiddenFromDesigner()
    {
        var (context, sut) = CreateSut(nameof(CreateCommentAsync_SuperAdminNonInternal_UsesAdminRelay_AndHiddenFromDesigner));
        await using (context)
        {
            var (orderId, superAdminId, designerUserId, clientUserId) = await SeedOrderWithPartiesAsync(context);

            var created = await sut.CreateCommentAsync(
                orderId,
                new CreateCommentRequestDto { Content = "Hello client", IsInternal = false },
                superAdminId,
                "SuperAdmin");

            Assert.Equal(CommentType.AdminRelay.ToString(), created.CommentType);
            Assert.False(created.IsInternal);
            Assert.True(created.VisibleToClient);

            var clientComments = await sut.GetOrderCommentsAsync(orderId, clientUserId, "Client");
            Assert.Contains(clientComments, c => c.Content == "Hello client");

            var designerComments = await sut.GetOrderCommentsAsync(orderId, designerUserId, "Designer");
            Assert.DoesNotContain(designerComments, c => c.Content == "Hello client");
        }
    }

    [Fact]
    public async Task CreateCommentAsync_SuperAdminInternal_VisibleToDesigner_NotToClient()
    {
        var (context, sut) = CreateSut(nameof(CreateCommentAsync_SuperAdminInternal_VisibleToDesigner_NotToClient));
        await using (context)
        {
            var (orderId, superAdminId, designerUserId, clientUserId) = await SeedOrderWithPartiesAsync(context);

            await sut.CreateCommentAsync(
                orderId,
                new CreateCommentRequestDto { Content = "Staff only", IsInternal = true },
                superAdminId,
                "SuperAdmin");

            var designerComments = await sut.GetOrderCommentsAsync(orderId, designerUserId, "Designer");
            Assert.Contains(designerComments, c => c.Content == "Staff only");

            var clientComments = await sut.GetOrderCommentsAsync(orderId, clientUserId, "Client");
            Assert.DoesNotContain(clientComments, c => c.Content == "Staff only");
        }
    }

    [Fact]
    public async Task GetOrderCommentsAsync_LegacyStaffGeneralVisibleToClient_HiddenFromDesigner()
    {
        var (context, sut) = CreateSut(nameof(GetOrderCommentsAsync_LegacyStaffGeneralVisibleToClient_HiddenFromDesigner));
        await using (context)
        {
            var (orderId, superAdminId, designerUserId, _) = await SeedOrderWithPartiesAsync(context);

            context.OrderComments.Add(new OrderComment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Content = "Legacy client message",
                CreatedBy = superAdminId,
                IsInternal = false,
                CommentType = CommentType.General,
                VisibleToClient = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var designerComments = await sut.GetOrderCommentsAsync(orderId, designerUserId, "Designer");
            Assert.DoesNotContain(designerComments, c => c.Content == "Legacy client message");
        }
    }
}
