using LogoDesignPortal.API.Hubs;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.API.Controllers;

/// <summary>
/// System health and monitoring endpoints for production operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SystemController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemController> _logger;

    public SystemController(
        ApplicationDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        IConfiguration configuration,
        ILogger<SystemController> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for monitoring. Returns status of database, SignalR, and file storage.
    /// Use GET /api/system/health to verify production deployment.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth()
    {
        var response = new HealthCheckResponse();

        // Database connection and critical tables
        try
        {
            await _dbContext.Database.CanConnectAsync();
            // Verify critical tables exist (catches missing migrations)
            _ = await _dbContext.Invoices.Take(1).CountAsync();
            _ = await _dbContext.ClientLogoPricings.Take(1).CountAsync();
            _ = await _dbContext.DesignerLogoPricings.Take(1).CountAsync();
            response.Database = "ok";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check: database connection or table access failed");
            response.Database = "error";
            response.DatabaseError = ex.Message;
        }

        // SignalR hub (basic availability - hub context exists)
        try
        {
            _ = _hubContext.Clients.All;
            response.SignalR = "ok";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check: SignalR hub unavailable");
            response.SignalR = "error";
        }

        // File storage
        try
        {
            var basePath = _configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
            var fullPath = Path.IsPathRooted(basePath) ? basePath : Path.Combine(Directory.GetCurrentDirectory(), basePath);
            if (Directory.Exists(fullPath))
            {
                var testFile = Path.Combine(fullPath, ".health-check");
                await System.IO.File.WriteAllTextAsync(testFile, "ok");
                System.IO.File.Delete(testFile);
                response.Storage = "ok";
            }
            else
            {
                Directory.CreateDirectory(fullPath);
                response.Storage = "ok";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check: file storage unavailable");
            response.Storage = "error";
        }

        return Ok(response);
    }

    public class HealthCheckResponse
    {
        public string Database { get; set; } = "unknown";
        public string? DatabaseError { get; set; }
        public string SignalR { get; set; } = "unknown";
        public string Storage { get; set; } = "unknown";
    }
}
