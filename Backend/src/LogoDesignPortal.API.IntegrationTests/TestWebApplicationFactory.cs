using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.API.IntegrationTests;

/// <summary>
/// WebApplicationFactory for API integration tests. Uses EF Core InMemory database.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Designer User.Id from seeded test data (for assign tests).</summary>
    public Guid DesignerUserId => TestDataIds.DesignerUserId;

    /// <summary>Client User.Id from seeded test data.</summary>
    public Guid ClientUserId => TestDataIds.ClientUserId;

    /// <summary>Admin User.Id from seeded test data.</summary>
    public Guid AdminUserId => TestDataIds.AdminUserId;

    /// <summary>Client profile Id for the seeded client user (LogoOrder.ClientId).</summary>
    public Guid ClientProfileId => TestDataIds.ClientProfileId;

    /// <summary>Designer profile Id for the seeded designer user.</summary>
    public Guid DesignerProfileId => TestDataIds.DesignerProfileId;

    /// <summary>Isolated in-memory database per factory instance (serialized via ICollectionFixture).</summary>
    private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid().ToString("N")[..8];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ConfigureTestServices runs AFTER application's ConfigureServices - ensures our DbContext overrides AddInfrastructure
        builder.ConfigureTestServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                            d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            foreach (var d in descriptorsToRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton<TestDataSeeder>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<TestDataSeeder>());
        });
    }
}
