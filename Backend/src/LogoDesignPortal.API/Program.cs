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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials for cookies/auth
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logo Design Portal API v1");
    });
}

// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// CORS must be before HTTPS redirection to handle preflight requests
app.UseCors("AllowAll");

// Only redirect to HTTPS in production, not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

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
            
            // Use async method and add timeout
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogInformation("Database does not exist. Creating database...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully.");
            }
            else
            {
                logger.LogInformation("Database connection verified.");
            }

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
        logger.LogWarning("Make sure SQL Server (LocalDB or full instance) is running.");
        logger.LogWarning("For LocalDB, run: sqllocaldb start MSSQLLocalDB");
        logger.LogWarning("Or update connection string in appsettings.json to use a full SQL Server instance.");
    }
});

app.Run();
