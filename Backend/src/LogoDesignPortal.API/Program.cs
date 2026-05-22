using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.Redis;
using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Health;
using LogoDesignPortal.Application;
using LogoDesignPortal.Application.BackgroundJobs;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.API.BackgroundJobs;
using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.API.Services;
using LogoDesignPortal.API.Hosting;
using LogoDesignPortal.API.Middleware;
using LogoDesignPortal.Application.Constants;
using LogoDesignPortal.Infrastructure;
using LogoDesignPortal.Infrastructure.Persistence;
using LogoDesignPortal.Infrastructure.Persistence.Seeding;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
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

ProductionSecretsValidator.Validate(builder.Configuration, builder.Environment);
EnvironmentConfigurationValidator.Validate(builder.Configuration, builder.Environment);

// Add services to the container - ensure camelCase for JSON (Angular expects it)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = UploadLimits.MaxMultipartBytes;
});
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI: Development only — never register in Production/Staging
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Logo Design Portal API",
            Version = "v1",
            Description = "Logo Design Business Web Portal — local development only"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
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
}

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1),
        RequireExpirationTime = true,
        RequireSignedTokens = true
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
                return Task.CompletedTask;
            }

            // Prefer explicit Bearer header over auth cookies (E2E/API clients send Bearer; cookies may be stale from a prior login).
            var authorization = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorization)
                && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var cookieService = context.HttpContext.RequestServices.GetService<IAuthCookieService>();
            var cookieToken = cookieService?.GetAccessTokenFromRequest(context.Request);
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;
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
    ScalabilityServiceRegistration.AddHangfireForPortal(
        builder.Services,
        builder.Configuration,
        builder.Environment,
        redisOk,
        scalabilityLogger);

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
builder.Services.Configure<AuthCookieOptions>(
    builder.Configuration.GetSection(AuthCookieOptions.SectionName));
builder.Services.AddSingleton<IAuthCookieService, AuthCookieService>();

builder.Services.Configure<ProductionSafetyOptions>(
    builder.Configuration.GetSection(ProductionSafetyOptions.SectionName));
builder.Services.Configure<InvoiceBrandingOptions>(
    builder.Configuration.GetSection(InvoiceBrandingOptions.SectionName));

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
builder.Services.AddSingleton<FileStorageHealthCheck>();
builder.Services.AddSingleton<SmtpConfigurationHealthCheck>();
builder.Services.AddSingleton<DiskSpaceHealthCheck>();
builder.Services.AddSingleton<MemoryPressureHealthCheck>();
builder.Services.AddSingleton<SignalRHealthCheck>();
builder.Services.AddSingleton<ProcessLivenessHealthCheck>();
builder.Services.Configure<ObservabilityOptions>(builder.Configuration.GetSection(ObservabilityOptions.SectionName));
builder.Services.AddPortalObservability(builder.Configuration, builder.Environment);

var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck<ProcessLivenessHealthCheck>("process", tags: new[] { "live" })
    .AddCheck<DatabaseSchemaReadinessHealthCheck>("database_schema", tags: new[] { "ready", "db" })
    .AddCheck<HangfireStorageHealthCheck>("hangfire", tags: new[] { "ready" })
    .AddCheck<FileStorageHealthCheck>("file_storage", tags: new[] { "ready" })
    .AddCheck<DiskSpaceHealthCheck>("disk_space", tags: new[] { "ready" })
    .AddCheck<SmtpConfigurationHealthCheck>("smtp", tags: new[] { "ready" })
    .AddCheck<MemoryPressureHealthCheck>("memory", tags: new[] { "ready" })
    .AddCheck<SignalRHealthCheck>("signalr", tags: new[] { "ready" });

var redisHealth = builder.Configuration.GetConnectionString("Redis") ?? builder.Configuration["Redis:Configuration"];
if (!string.IsNullOrWhiteSpace(redisHealth))
    healthChecks.AddRedis(redisHealth, name: "redis", tags: new[] { "ready" });

// File storage initialization at startup (ensures directories exist before first upload)
builder.Services.AddSingleton<LogoDesignPortal.API.Services.FileStorageInitializer>();

// Scheduled maintenance (Hangfire recurring jobs — not IHostedService — avoids duplicate work per instance when Redis is used).
builder.Services.AddSingleton<LogoDesignPortal.API.Services.OrphanFileCleanupService>();
builder.Services.AddSingleton<LogoDesignPortal.API.Services.BillingAutoInvoiceService>();

// CORS: built-in production origins + Cors:AllowedOrigins from appsettings / env (see CorsAllowedOrigins).
// OPTIONS preflight is handled by CORS middleware; UseCors must run before UseAuthentication
var corsOrigins = CorsAllowedOrigins.Resolve(builder.Configuration, builder.Environment);
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsAllowedOrigins.PolicyName, policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))
        .AllowCredentials());
});

// Forwarded headers for IIS deployment (X-Forwarded-Proto, X-Forwarded-For)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Swagger: Development only — never expose API surface on production/staging hosts
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logo Design Portal API v1");
        c.DocumentTitle = "Logo Design Portal API (Development)";
    });
}

// Forwarded headers first (required for IIS - correct scheme/host when behind reverse proxy)
app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeadersMiddleware>();

// CORS must run BEFORE authentication so preflight OPTIONS requests succeed without 401
app.UseCors(CorsAllowedOrigins.PolicyName);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SlowRequestPerformanceMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        diagnosticContext.Set("UserId", string.IsNullOrEmpty(userId) ? null : userId);
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        diagnosticContext.Set("UserRole", string.IsNullOrEmpty(role) ? null : role);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var cid) && cid is string correlationId)
            diagnosticContext.Set("CorrelationId", correlationId);
        if (httpContext.Request.RouteValues.TryGetValue("orderId", out var oid) && oid != null)
            diagnosticContext.Set("OrderId", oid.ToString());
        if (httpContext.Request.RouteValues.TryGetValue("id", out var id) &&
            httpContext.Request.Path.Value?.Contains("/invoices", StringComparison.OrdinalIgnoreCase) == true)
            diagnosticContext.Set("InvoiceId", id.ToString());
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
app.UseMiddleware<CsrfValidationMiddleware>();
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

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

// Initialize file storage directories at startup (Files, Files/Temporary, Files/Permanent)
var fileStorageInitializer = app.Services.GetRequiredService<LogoDesignPortal.API.Services.FileStorageInitializer>();
fileStorageInitializer.Initialize();

// Recurring jobs (skipped in test environment).
ScalabilityServiceRegistration.AddRecurringJobsIfEnabled(app.Environment);

if (args.Contains("--repair-database", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseRepair");
    if (context.Database.IsRelational())
    {
        await context.Database.MigrateAsync();
    }
    await DatabaseStartupSeeder.EnsureRolesPermissionsAndLinksAsync(context, logger);
    Log.Information("Database repair completed (roles, permissions, role-permission links).");
    return;
}

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
