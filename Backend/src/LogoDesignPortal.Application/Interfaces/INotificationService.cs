using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Creates a notification for a single user. All notifications must be created through this service.
    /// </summary>
    Task<NotificationResponseDto> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? orderId = null, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null);

    /// <summary>
    /// Creates a notification for a single user (alias for CreateNotificationAsync for clarity).
    /// </summary>
    Task<NotificationResponseDto> CreateNotificationForUserAsync(Guid userId, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null);

    /// <summary>
    /// Creates the same notification for multiple users.
    /// </summary>
    Task<List<NotificationResponseDto>> CreateNotificationForUsersAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null);

    /// <summary>
    /// Creates the same notification for all users with the specified role.
    /// </summary>
    Task<List<NotificationResponseDto>> CreateNotificationForRoleAsync(string roleName, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null);

    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);

    /// <summary>
    /// Gets user notifications with server-side pagination, filtering, and search.
    /// </summary>
    Task<NotificationListResponseDto> GetUserNotificationsPaginatedAsync(
        Guid userId,
        int limit = 20,
        int offset = 0,
        bool unreadOnly = false,
        string? search = null,
        string? referenceType = null,
        string? type = null);
    Task<NotificationResponseDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId);
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
}
