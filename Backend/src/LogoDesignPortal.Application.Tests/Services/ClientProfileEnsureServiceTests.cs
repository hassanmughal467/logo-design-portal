using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class ClientProfileEnsureServiceTests
{
    private static ApplicationDbContext CreateContext(string db) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(db).Options);

    private static ClientProfileEnsureService CreateSut(ApplicationDbContext ctx) =>
        new(ctx, Mock.Of<IReadModelCacheVersions>(), Mock.Of<ILogger<ClientProfileEnsureService>>());

    [Fact]
    public async Task Ensure_ReturnsExisting_WhenProfileActive()
    {
        var ctx = CreateContext(nameof(Ensure_ReturnsExisting_WhenProfileActive));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "a@a.com", PasswordHash = "x" };
        var profile = new ClientProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user, CompanyName = "Co" };
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        ctx.ClientProfiles.Add(profile);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.EnsureForClientUserAsync(user.Id);

        Assert.Equal(profile.Id, result.Id);
    }

    [Fact]
    public async Task Ensure_CreatesProfile_WhenClientHasNoRow()
    {
        var ctx = CreateContext(nameof(Ensure_CreatesProfile_WhenClientHasNoRow));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            Role = role,
            Email = "b@b.com",
            FirstName = "Bo",
            LastName = "Tau",
            PasswordHash = "x"
        };
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.EnsureForClientUserAsync(user.Id);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Contains("Bo", result.CompanyName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Ensure_RestoresSoftDeletedProfile_AndFillsEmptyCompanyName()
    {
        var ctx = CreateContext(nameof(Ensure_RestoresSoftDeletedProfile_AndFillsEmptyCompanyName));
        var role = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            Role = role,
            Email = "c@c.com",
            FirstName = "Cy",
            LastName = "C",
            PasswordHash = "x"
        };
        var profile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CompanyName = "",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        ctx.ClientProfiles.Add(profile);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.EnsureForClientUserAsync(user.Id);

        Assert.Equal(profile.Id, result.Id);
        Assert.False(result.IsDeleted);
        Assert.False(string.IsNullOrWhiteSpace(result.CompanyName));
    }

    [Fact]
    public async Task Ensure_AcceptsClientRole_CaseInsensitive()
    {
        var ctx = CreateContext(nameof(Ensure_AcceptsClientRole_CaseInsensitive));
        var role = new Role { Id = Guid.NewGuid(), Name = "CLIENT" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "e@e.com", PasswordHash = "x" };
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.EnsureForClientUserAsync(user.Id);

        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task Ensure_ThrowsUnauthorized_WhenUserDoesNotExist()
    {
        var ctx = CreateContext(nameof(Ensure_ThrowsUnauthorized_WhenUserDoesNotExist));
        var sut = CreateSut(ctx);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.EnsureForClientUserAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Ensure_ThrowsForbidden_WhenNotClientRole()
    {
        var ctx = CreateContext(nameof(Ensure_ThrowsForbidden_WhenNotClientRole));
        var role = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var user = new User { Id = Guid.NewGuid(), RoleId = role.Id, Role = role, Email = "d@d.com", PasswordHash = "x" };
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => sut.EnsureForClientUserAsync(user.Id));
    }
}
