using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
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
        var userId = User.GetUserIdOrThrow();

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
        var userId = User.GetUserIdOrThrow();
        var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
        return Ok(new { count });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        var userId = User.GetUserIdOrThrow();
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
        var userId = User.GetUserIdOrThrow();
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
        var userId = User.GetUserIdOrThrow();
        await _notificationService.MarkAllNotificationsAsReadAsync(userId);
        return Ok(new { message = "All notifications marked as read." });
    }
}
