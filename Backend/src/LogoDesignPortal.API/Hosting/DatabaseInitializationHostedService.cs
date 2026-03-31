using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Hosting;

public sealed class DatabaseInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseInitializationHostedService> _logger;
    private readonly DatabaseInitializationOptions _options;

    public DatabaseInitializationHostedService(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        IOptions<DatabaseInitializationOptions> options,
        ILogger<DatabaseInitializationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
        _options = options?.Value ?? new DatabaseInitializationOptions();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_environment.IsEnvironment("Testing"))
            return Task.CompletedTask;

        if (!_options.RunAfterStartup)
        {
            _logger.LogInformation("Database initialization skipped (Database:RunAfterStartup is false). Apply migrations manually or enable the option.");
            return Task.CompletedTask;
        }

        _ = RunInitializationFireAndForgetAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task RunInitializationFireAndForgetAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(_options.StartDelaySeconds, 0, 300)), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseInitializationHostedService>>();

            logger.LogInformation("Background database initialization starting...");

            if (context.Database.IsRelational())
            {
                var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
                if (pending.Any())
                {
                    logger.LogInformation("Applying pending migrations: {Migrations}", string.Join(", ", pending));
                    await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("Database schema is up to date.");
                }
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Non-relational provider: EnsureCreated completed (e.g. in-memory tests).");
            }

            await EnsureStandardRolesAsync(context, logger, cancellationToken).ConfigureAwait(false);
            await SeedPaymentSettingsAsync(context, logger, cancellationToken).ConfigureAwait(false);
            await EnsureSuperAdminUserAsync(context, logger, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Background database initialization completed.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogCritical(ex,
                "Background database initialization failed. The API is running but the schema may be incomplete; fix the database and restart or run migrations manually.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task EnsureStandardRolesAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        var standardRoles = new (Guid Id, string Name, string Description)[]
        {
            (SeededRoleIds.SuperAdmin, "SuperAdmin", "Full system access with all permissions"),
            (SeededRoleIds.Admin, "Admin", "Administrative access with restricted client data access"),
            (SeededRoleIds.Designer, "Designer", "Designer access without client identity information"),
            (SeededRoleIds.Client, "Client", "Client access to their own data")
        };

        foreach (var (id, name, description) in standardRoles)
        {
            var byId = await context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct).ConfigureAwait(false);
            if (byId != null)
            {
                if (byId.IsDeleted)
                {
                    byId.IsDeleted = false;
                    byId.DeletedAt = null;
                    byId.DeletedBy = null;
                    await context.SaveChangesAsync(ct).ConfigureAwait(false);
                    logger.LogInformation("Restored soft-deleted role {RoleName}.", name);
                }
                continue;
            }

            var byName = await context.Roles.FirstOrDefaultAsync(r => r.Name == name, ct).ConfigureAwait(false);
            if (byName != null)
            {
                if (byName.IsDeleted)
                {
                    byName.IsDeleted = false;
                    byName.DeletedAt = null;
                    byName.DeletedBy = null;
                    await context.SaveChangesAsync(ct).ConfigureAwait(false);
                    logger.LogInformation("Restored soft-deleted role {RoleName} (matched by name).", name);
                }
                else
                    logger.LogWarning("Role {RoleName} exists with non-standard Id {RoleId}. Skipping insert by reserved id.", name, byName.Id);
                continue;
            }

            context.Roles.Add(new Role
            {
                Id = id,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            logger.LogInformation("Seeded standard role {RoleName}.", name);
        }
    }

    private static async Task SeedPaymentSettingsAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        try
        {
            var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com", ct).ConfigureAwait(false);
            var createdBy = superAdmin?.Id ?? Guid.Empty;

            var paymentSettings = new[]
            {
                new { Key = "PayPalClientId", Value = "DUMMY_PAYPAL_CLIENT_ID_FOR_TESTING", Category = "Payment" },
                new { Key = "PayPalClientSecret", Value = "DUMMY_PAYPAL_CLIENT_SECRET_FOR_TESTING", Category = "Payment" },
                new { Key = "PayPalUseSandbox", Value = "true", Category = "Payment" },
                new { Key = "PayPalWebhookId", Value = "", Category = "Payment" },
                new { Key = "WiseApiKey", Value = "DUMMY_WISE_API_KEY_FOR_TESTING", Category = "Payment" },
                new { Key = "WiseProfileId", Value = "DUMMY_WISE_PROFILE_ID_FOR_TESTING", Category = "Payment" },
                new { Key = "BankName", Value = "Demo Bank", Category = "Payment" },
                new { Key = "AccountHolderName", Value = "Hawk Merchandising", Category = "Payment" },
                new { Key = "AccountNumber", Value = "1234567890", Category = "Payment" },
                new { Key = "IBAN", Value = "GB82WEST12345698765432", Category = "Payment" },
                new { Key = "SWIFT", Value = "DEMOBANK123", Category = "Payment" },
                new { Key = "RoutingNumber", Value = "123456789", Category = "Payment" },
                new { Key = "BranchAddress", Value = "123 Main Street, City, Country", Category = "Payment" },
                new { Key = "BankCurrency", Value = "USD", Category = "Payment" }
            };

            foreach (var setting in paymentSettings)
            {
                var existing = await context.Settings
                    .FirstOrDefaultAsync(s => s.Key == setting.Key && s.Category == setting.Category && !s.IsDeleted, ct)
                    .ConfigureAwait(false);

                if (existing == null)
                {
                    context.Settings.Add(new Settings
                    {
                        Id = Guid.NewGuid(),
                        Key = setting.Key,
                        Value = setting.Value,
                        Category = setting.Category,
                        Description = $"Dummy {setting.Key} for testing purposes",
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });
                    logger.LogInformation("Seeded payment setting: {Key}", setting.Key);
                }
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error seeding payment settings. They may need to be configured manually.");
        }
    }

    private static async Task EnsureSuperAdminUserAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct).ConfigureAwait(false);
        if (superAdminRole == null)
            return;

        var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com", ct).ConfigureAwait(false);
        if (superAdmin == null)
        {
            superAdmin = new User
            {
                Id = Guid.NewGuid(),
                Email = "superadmin@logodesign.com",
                FirstName = "Super",
                LastName = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
                RoleId = superAdminRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(superAdmin);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            logger.LogInformation("SuperAdmin user created.");
        }
    }
}

public sealed class DatabaseInitializationOptions
{
    public const string SectionName = "Database";

    /// <summary>When false, no automatic migration/seed runs in the background (use external tooling or admin pipeline).</summary>
    public bool RunAfterStartup { get; set; } = true;

    /// <summary>Lets Kestrel finish binding before heavy DB work.</summary>
    public int StartDelaySeconds { get; set; } = 2;
}
