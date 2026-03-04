using LogoDesignPortal.Application;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.API.Middleware;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
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
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
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
});

builder.Services.AddAuthorization();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS - origins from config (appsettings.Production.json when deployed to IIS)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200", "http://localhost:83", "http://209.209.42.42", "http://209.209.42.42:83", "http://209.209.42.42:4200", "https://admin.hawkmerchandising.com" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin", policy => policy
        .WithOrigins("https://admin.hawkmerchandising.com", "http://admin.hawkmerchandising.com", "http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());

    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials for cookies/auth
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline - Swagger enabled for testing on IIS
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logo Design Portal API v1");
});

// Global Exception Handler (must be first to catch all errors)
app.UseMiddleware<ExceptionMiddleware>();

// CORS must be very early to handle preflight OPTIONS requests
app.UseCors("AllowAdmin");

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

// Ensure database is created and seeded (async to avoid blocking startup)
_ = Task.Run(async () =>
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            logger.LogInformation("Initializing database...");
            
            // Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date.");
            }

            // Seed payment settings with dummy values if they don't exist
            await SeedPaymentSettingsAsync(context, logger);

            // Seed or reset SuperAdmin user
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole != null)
            {
                var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com");
                if (superAdmin == null)
                {
                    // Create new SuperAdmin
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
    }
    catch (Exception ex)
    {
        // Log error but don't crash the application
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Error initializing database. The application will continue, but database operations may fail.");
        logger.LogWarning("Make sure MySQL is running and connection string in appsettings.json is correct.");
    }
});

app.Run();

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
