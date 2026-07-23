using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.Redis;
using LogoDesignPortal.API.Health;
using LogoDesignPortal.Application;
using LogoDesignPortal.Application.BackgroundJobs;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.API.BackgroundJobs;
using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Hosting;
using LogoDesignPortal.API.Middleware;
using LogoDesignPortal.Infrastructure;
using LogoDesignPortal.Infrastructure.Persistence;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName();
});

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

var redisConnectionForSignalR =
    builder.Configuration.GetConnectionString("Redis")
    ?? builder.Configuration["Redis:Configuration"];

using (var scalabilityLoggerFactory = LoggerFactory.Create(b => b.AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole()))
{
    var scalabilityLogger = scalabilityLoggerFactory.CreateLogger("Scalability");
    var redisOk = ScalabilityServiceRegistration.AddDistributedCacheEpochsAndRateLimiter(
        builder.Services,
        builder.Configuration,
        builder.Environment,
        scalabilityLogger);
    builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
    ScalabilityServiceRegistration.AddHangfireForPortal(builder.Services, builder.Configuration, builder.Environment, redisOk);

    var signalR = builder.Services.AddSignalR();
    if (redisOk && !string.IsNullOrWhiteSpace(redisConnectionForSignalR) && !builder.Environment.IsEnvironment("Testing"))
    {
        signalR.AddStackExchangeRedis(redisConnectionForSignalR, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("ldp:signalr");
        });
    }
}

builder.Services.AddSingleton<LogoDesignPortal.Application.Interfaces.IRealtimeNotificationSender, LogoDesignPortal.API.Services.SignalRRealtimeNotificationSender>();
builder.Services.AddSingleton<LogoDesignPortal.Application.Interfaces.IRealtimeEntityUpdateSender, LogoDesignPortal.API.Services.SignalRRealtimeEntityUpdateSender>();

builder.Services.Configure<DatabaseInitializationOptions>(
    builder.Configuration.GetSection(DatabaseInitializationOptions.SectionName));
builder.Services.AddHostedService<DatabaseInitializationHostedService>();

// Production safety kill-switch configuration
builder.Services.Configure<ProductionSafetyOptions>(
    builder.Configuration.GetSection(ProductionSafetyOptions.SectionName));

// Background jobs: must register before AddApplication (Auth/Notification require IBackgroundJobScheduler).
if (builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddSingleton<IBackgroundJobScheduler, NullBackgroundJobScheduler>();
else
    builder.Services.AddSingleton<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
builder.Services.AddTransient<EmailHangfireJobs>();
builder.Services.AddTransient<MaintenanceHangfireJobs>();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<DatabaseSchemaReadinessHealthCheck>();
var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck<DatabaseSchemaReadinessHealthCheck>("database_schema", tags: new[] { "ready", "db" })
    .AddCheck<HangfireStorageHealthCheck>("hangfire", tags: new[] { "ready" });

var redisHealth = builder.Configuration.GetConnectionString("Redis") ?? builder.Configuration["Redis:Configuration"];
if (!string.IsNullOrWhiteSpace(redisHealth))
    healthChecks.AddRedis(redisHealth, name: "redis", tags: new[] { "ready" });

// File storage initialization at startup (ensures directories exist before first upload)
builder.Services.AddSingleton<LogoDesignPortal.API.Services.FileStorageInitializer>();

// Scheduled maintenance (Hangfire recurring jobs — not IHostedService — avoids duplicate work per instance when Redis is used).
builder.Services.AddSingleton<LogoDesignPortal.API.Services.OrphanFileCleanupService>();
builder.Services.AddSingleton<LogoDesignPortal.API.Services.BillingAutoInvoiceService>();

// CORS - allow frontend domain (admin.hawkmerchandising.com) and local dev origins
// OPTIONS preflight is handled by CORS middleware; UseCors must run before UseAuthentication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin", policy => policy
        .WithOrigins(
            "http://admin.hawkmerchandising.com",
            "https://admin.hawkmerchandising.com",
            "http://localhost:4200",
            "https://localhost:4200",
            // Local dev / Playwright: some environments resolve or open the app as 127.0.0.1 (browser Origin must match exactly).
            "http://127.0.0.1:4200",
            "https://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)) // Cache preflight for 24h
        .AllowCredentials()); // Required for SignalR WebSocket and JWT cookies
});

// Forwarded headers for IIS deployment (X-Forwarded-Proto, X-Forwarded-For).
// SECURITY: do NOT clear KnownNetworks/KnownProxies - an empty allowlist trusts X-Forwarded-For from
// any client, letting requests spoof their own IP (this previously defeated IP-based rate limiting).
// Keep the default (loopback-only) trust, plus any explicitly configured trusted proxy/load balancer.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    var trustedProxies = builder.Configuration.GetSection("ForwardedHeaders:TrustedProxies").Get<string[]>() ?? Array.Empty<string>();
    foreach (var proxy in trustedProxies)
    {
        if (System.Net.IPAddress.TryParse(proxy.Trim(), out var proxyIp))
            options.KnownProxies.Add(proxyIp);
    }
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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SlowRequestPerformanceMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        diagnosticContext.Set("UserId", string.IsNullOrEmpty(userId) ? null : userId);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
    };
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms; UserId={UserId}";
});

// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// Only redirect to HTTPS in production, not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

// Security Middleware (after CORS and auth to allow preflight requests)
app.UseMiddleware<RateLimitingMiddleware>();
// InputSanitizationMiddleware temporarily disabled - re-enable after proper stream handling implementation
// app.UseMiddleware<InputSanitizationMiddleware>();

app.MapControllers();
app.MapHub<LogoDesignPortal.API.Hubs.NotificationHub>("/hubs/notifications");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

// Liveness: process is up and can handle requests. Excludes all registered checks (no DB/Redis/
// Hangfire/file-storage dependency) so it can't be dragged down by a downstream outage.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

// Initialize file storage directories at startup (Files, Files/Temporary, Files/Permanent)
var fileStorageInitializer = app.Services.GetRequiredService<LogoDesignPortal.API.Services.FileStorageInitializer>();
fileStorageInitializer.Initialize();

// Recurring jobs (skipped in test environment).
ScalabilityServiceRegistration.AddRecurringJobsIfEnabled(app.Environment);

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
