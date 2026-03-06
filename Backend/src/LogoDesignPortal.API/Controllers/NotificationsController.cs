using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IApplicationDbContext context,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Diagnostic endpoint for troubleshooting notification creation. SuperAdmin only.
    /// Returns admin role count, admin user count, and recent notification count.
    /// </summary>
    [HttpGet("debug")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebugInfo()
    {
        var adminRoleIds = await _context.Roles
            .Where(r => r.Name == "Admin" || r.Name == "SuperAdmin")
            .Select(r => r.Id)
            .ToListAsync();

        var adminUserCount = await _context.Users
            .Where(u => !u.IsDeleted && adminRoleIds.Contains(u.RoleId))
            .CountAsync();

        var notificationCount = await _context.Notifications
            .Where(n => !n.IsDeleted)
            .CountAsync();

        return Ok(new
        {
            adminRoleCount = adminRoleIds.Count,
            adminUserCount,
            notificationCount,
            message = adminUserCount == 0
                ? "No Admin/SuperAdmin users found. Notifications will not be created for new orders. Check Users.RoleId matches Admin/SuperAdmin role."
                : "OK"
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        [FromQuery] string? search = null,
        [FromQuery] string? referenceType = null,
        [FromQuery] string? type = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Use paginated endpoint when limit or offset is explicitly provided, or when filters/search are used
        var usePaginated = limit.HasValue || offset.HasValue || !string.IsNullOrWhiteSpace(search) ||
            !string.IsNullOrWhiteSpace(referenceType) || !string.IsNullOrWhiteSpace(type);

        if (usePaginated)
        {
            var effectiveLimit = limit.HasValue && limit.Value > 0 ? Math.Min(limit.Value, 100) : 20;
            var effectiveOffset = offset ?? 0;
            var result = await _notificationService.GetUserNotificationsPaginatedAsync(
                userId,
                effectiveLimit,
                effectiveOffset,
                unreadOnly,
                search,
                referenceType,
                type);
            return Ok(result);
        }

        // Backward compatibility: return simple list when no pagination/filters
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
        return Ok(new { count });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notification = await _notificationService.GetNotificationByIdAsync(id, userId);
        
        if (notification == null)
        {
            return NotFound(new { error = "Notification not found." });
        }

        return Ok(notification);
    }

    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);
        
        if (!result)
        {
            return NotFound(new { error = "Notification not found." });
        }

        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllNotificationsAsReadAsync(userId);
        return Ok(new { message = "All notifications marked as read." });
    }
}
