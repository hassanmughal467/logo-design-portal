using LogoDesignPortal.Application;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.API.Middleware;
using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Validate connection string in Production - fail fast with clear message (common IIS 500.30 cause)
if (!builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString) ||
        connectionString.Contains("YOUR_MYSQL_PASSWORD", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("REPLACE_IN_WEB_CONFIG", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("REPLACE_WITH_ACTUAL", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Database connection string not configured for Production. " +
            "Set ConnectionStrings__DefaultConnection in web.config <environmentVariables> or appsettings.Production.json. " +
            "Example: Server=localhost;Port=3306;Database=LogoDesignPortalDb;User=root;Password=YOUR_ACTUAL_PASSWORD;");
    }
}

// Add services to the container - ensure camelCase for JSON (Angular expects it)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Logo Design Portal API",
        Version = "v1",
        Description = "Logo Design Business Web Portal - Phase 1 Backend Foundation"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured. Use User Secrets (dev) or environment variables (production).");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("JWT Key must be at least 32 characters for security. Use a strong random key in production.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    // SignalR uses WebSockets - token must come from query string (browsers don't support custom headers for WS)
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// SignalR for real-time notifications and entity updates
builder.Services.AddSignalR();
builder.Services.AddSingleton<LogoDesignPortal.Application.Interfaces.IRealtimeNotificationSender, LogoDesignPortal.API.Services.SignalRRealtimeNotificationSender>();
builder.Services.AddSingleton<LogoDesignPortal.Application.Interfaces.IRealtimeEntityUpdateSender, LogoDesignPortal.API.Services.SignalRRealtimeEntityUpdateSender>();

// Production safety kill-switch configuration
builder.Services.Configure<ProductionSafetyOptions>(
    builder.Configuration.GetSection(ProductionSafetyOptions.SectionName));

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// File storage initialization at startup (ensures directories exist before first upload)
builder.Services.AddSingleton<LogoDesignPortal.API.Services.FileStorageInitializer>();

// Orphan file cleanup - runs every 24 hours, scans preview (Temporary) storage only
builder.Services.AddHostedService<LogoDesignPortal.API.Services.OrphanFileCleanupService>();

// Billing auto-invoice - runs daily, generates invoices for Weekly (Mondays) and Monthly (1st) clients
builder.Services.AddHostedService<LogoDesignPortal.API.Services.BillingAutoInvoiceService>();

// CORS - allow frontend domain (admin.hawkmerchandising.com) and local dev origins
// OPTIONS preflight is handled by CORS middleware; UseCors must run before UseAuthentication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin", policy => policy
        .WithOrigins(
            "http://admin.hawkmerchandising.com",
            "https://admin.hawkmerchandising.com",
            "http://localhost:4200",
            "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)) // Cache preflight for 24h
        .AllowCredentials()); // Required for SignalR WebSocket and JWT cookies
});

// Forwarded headers for IIS deployment (X-Forwarded-Proto, X-Forwarded-For)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline - Swagger enabled for testing on IIS
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logo Design Portal API v1");
});

// Forwarded headers first (required for IIS - correct scheme/host when behind reverse proxy)
app.UseForwardedHeaders();

// CORS must run BEFORE authentication so preflight OPTIONS requests succeed without 401
app.UseCors("AllowAdmin");

// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// Only redirect to HTTPS in production, not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Security Middleware (after CORS and auth to allow preflight requests)
app.UseMiddleware<RateLimitingMiddleware>();
// InputSanitizationMiddleware temporarily disabled - re-enable after proper stream handling implementation
// app.UseMiddleware<InputSanitizationMiddleware>();

app.MapControllers();
app.MapHub<LogoDesignPortal.API.Hubs.NotificationHub>("/hubs/notifications");

// Initialize file storage directories at startup (Files, Files/Temporary, Files/Permanent)
var fileStorageInitializer = app.Services.GetRequiredService<LogoDesignPortal.API.Services.FileStorageInitializer>();
fileStorageInitializer.Initialize();

// Apply migrations and seed data BEFORE accepting requests (prevents 500s from incomplete schema)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        logger.LogInformation("Initializing database...");

        // Apply pending migrations (blocking - ensures schema is ready before first request)
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("Database is up to date.");
        }

        // Standard roles (fixed GUIDs) — SuperAdmin, Admin, Designer, Client.
        await EnsureStandardRolesAsync(context, logger);

        // Seed payment settings with dummy values if they don't exist
        await SeedPaymentSettingsAsync(context, logger);

        // Seed or reset SuperAdmin user
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole != null)
        {
            var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com");
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
                await context.SaveChangesAsync();
                logger.LogInformation("SuperAdmin user created.");
            }
        }

        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed. Application will not start.");
        logger.LogWarning("Ensure MySQL is running and connection string in appsettings.json (or env) is correct.");
        throw; // Fail startup - do not serve requests with incomplete/missing schema
    }
}

app.Run();

static async Task EnsureStandardRolesAsync(ApplicationDbContext context, ILogger logger)
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
        var byId = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (byId != null)
        {
            if (byId.IsDeleted)
            {
                byId.IsDeleted = false;
                byId.DeletedAt = null;
                byId.DeletedBy = null;
                await context.SaveChangesAsync();
                logger.LogInformation("Restored soft-deleted role {RoleName}.", name);
            }
            continue;
        }

        var byName = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        if (byName != null)
        {
            if (byName.IsDeleted)
            {
                byName.IsDeleted = false;
                byName.DeletedAt = null;
                byName.DeletedBy = null;
                await context.SaveChangesAsync();
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
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded standard role {RoleName}.", name);
    }
}

// Helper method to seed payment settings
static async Task SeedPaymentSettingsAsync(ApplicationDbContext context, ILogger logger)
{
    try
    {
        var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com");
        var createdBy = superAdmin?.Id ?? Guid.Empty;

        // Payment settings to seed
        var paymentSettings = new[]
        {
            // PayPal Settings
            new { Key = "PayPalClientId", Value = "DUMMY_PAYPAL_CLIENT_ID_FOR_TESTING", Category = "Payment" },
            new { Key = "PayPalClientSecret", Value = "DUMMY_PAYPAL_CLIENT_SECRET_FOR_TESTING", Category = "Payment" },
            new { Key = "PayPalUseSandbox", Value = "true", Category = "Payment" },
            new { Key = "PayPalWebhookId", Value = "", Category = "Payment" },
            
            // Wise Settings
            new { Key = "WiseApiKey", Value = "DUMMY_WISE_API_KEY_FOR_TESTING", Category = "Payment" },
            new { Key = "WiseProfileId", Value = "DUMMY_WISE_PROFILE_ID_FOR_TESTING", Category = "Payment" },
            
            // Bank Details
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
                .FirstOrDefaultAsync(s => s.Key == setting.Key && s.Category == setting.Category && !s.IsDeleted);

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
                logger.LogInformation($"Seeded payment setting: {setting.Key}");
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Payment settings seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error seeding payment settings. They may need to be configured manually.");
    }
}
