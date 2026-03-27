using LogoDesignPortal.Domain.Constants;
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
        // Align with Program.cs / EF seed roles (fixed GUIDs). Do not insert duplicate role rows.
        if (context.Users.Any(u => u.Email == "client@test.com"))
            return;

        var testPassword = "Test@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(testPassword, BCrypt.Net.BCrypt.GenerateSalt(10));

        var superAdminRole = context.Roles.First(r => r.Id == SeededRoleIds.SuperAdmin);
        var adminRole = context.Roles.First(r => r.Id == SeededRoleIds.Admin);
        var clientRole = context.Roles.First(r => r.Id == SeededRoleIds.Client);
        var designerRole = context.Roles.First(r => r.Id == SeededRoleIds.Designer);

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = "superadmin@test.com",
            FirstName = "Super",
            LastName = "Admin",
            PasswordHash = passwordHash,
            RoleId = superAdminRole.Id,
            Role = superAdminRole,
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
            RoleId = adminRole.Id,
            Role = adminRole,
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
            RoleId = clientRole.Id,
            Role = clientRole,
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
            RoleId = designerRole.Id,
            Role = designerRole,
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
        TestDataIds.ClientProfileId = clientProfile.Id;
    }
}

/// <summary>Stores seeded user IDs for tests to access.</summary>
public static class TestDataIds
{
    public static Guid DesignerUserId { get; set; }
    public static Guid ClientUserId { get; set; }
    public static Guid AdminUserId { get; set; }
    public static Guid ClientProfileId { get; set; }
}
