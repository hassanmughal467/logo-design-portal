using LogoDesignPortal.Application.DTOs.Notifications;

namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Sends real-time notifications to connected clients via SignalR.
/// Implemented by the API layer; Application layer uses this for push delivery.
/// </summary>
public interface IRealtimeNotificationSender
{
    /// <summary>
    /// Sends a notification to a single user via SignalR.
    /// </summary>
    Task SendNotificationAsync(string userId, string title, string message, string notificationType, Guid? orderId = null);

    /// <summary>
    /// Sends a full notification DTO to a single user via SignalR.
    /// </summary>
    Task SendNotificationToUserAsync(Guid userId, NotificationResponseDto notification);

    /// <summary>
    /// Sends a full notification DTO to multiple users via SignalR.
    /// </summary>
    Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, NotificationResponseDto notification);
}
