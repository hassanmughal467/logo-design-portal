using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LogoDesignPortal.API.IntegrationTests;

/// <summary>
/// Seeds test data when the test host starts. Runs after DbContext is configured.
/// </summary>
public class TestDataSeeder : IHostedService
{
    private readonly IServiceProvider _services;

    public TestDataSeeder(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        SeedTestData(context);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void SeedTestData(ApplicationDbContext context)
    {
        if (context.Users.Any())
            return;

        var roles = new[]
        {
            new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" },
            new Role { Id = Guid.NewGuid(), Name = "Admin" },
            new Role { Id = Guid.NewGuid(), Name = "Client" },
            new Role { Id = Guid.NewGuid(), Name = "Designer" }
        };
        context.Roles.AddRange(roles);

        var testPassword = "Test@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(testPassword, BCrypt.Net.BCrypt.GenerateSalt(10));

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = "superadmin@test.com",
            FirstName = "Super",
            LastName = "Admin",
            PasswordHash = passwordHash,
            RoleId = roles[0].Id,
            Role = roles[0],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = passwordHash,
            RoleId = roles[1].Id,
            Role = roles[1],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var client = new User
        {
            Id = Guid.NewGuid(),
            Email = "client@test.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = passwordHash,
            RoleId = roles[2].Id,
            Role = roles[2],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var designer = new User
        {
            Id = Guid.NewGuid(),
            Email = "designer@test.com",
            FirstName = "Test",
            LastName = "Designer",
            PasswordHash = passwordHash,
            RoleId = roles[3].Id,
            Role = roles[3],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.AddRange(superAdmin, admin, client, designer);

        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = client.Id,
            CompanyName = "Test Company",
            ContactName = "Test Client",
            CreatedAt = DateTime.UtcNow
        };
        var designerProfile = new DesignerProfile
        {
            Id = Guid.NewGuid(),
            UserId = designer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.ClientProfiles.Add(clientProfile);
        context.DesignerProfiles.Add(designerProfile);

        context.SaveChanges();

        TestDataIds.DesignerUserId = designer.Id;
        TestDataIds.ClientUserId = client.Id;
        TestDataIds.AdminUserId = admin.Id;
    }
}

/// <summary>Stores seeded user IDs for tests to access.</summary>
public static class TestDataIds
{
    public static Guid DesignerUserId { get; set; }
    public static Guid ClientUserId { get; set; }
    public static Guid AdminUserId { get; set; }
}
