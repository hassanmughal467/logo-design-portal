using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDto> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? orderId = null);
    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<NotificationResponseDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId);
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
}
