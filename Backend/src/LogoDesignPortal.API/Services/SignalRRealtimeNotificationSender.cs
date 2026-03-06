using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LogoDesignPortal.API.Services;

public class SignalRRealtimeNotificationSender : IRealtimeNotificationSender
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;

    public SignalRRealtimeNotificationSender(IHubContext<Hubs.NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string notificationType, Guid? orderId = null)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = Guid.Empty,
                    title,
                    message,
                    type = notificationType,
                    orderId = orderId,
                    referenceType = "Order",
                    referenceId = orderId
                });
        }
        catch
        {
            // Real-time push failure must never affect notification persistence
            // Clients will receive via polling fallback
        }
    }

    public async Task SendNotificationToUserAsync(Guid userId, NotificationResponseDto notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    orderId = notification.OrderId,
                    referenceType = notification.ReferenceType,
                    referenceId = notification.ReferenceId,
                    isRead = notification.IsRead,
                    createdAt = notification.CreatedAt,
                    aggregationCount = notification.AggregationCount,
                    lastOccurrenceAt = notification.LastOccurrenceAt
                });
        }
        catch
        {
            // Non-critical
        }
    }

    public async Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, NotificationResponseDto notification)
    {
        try
        {
            var payload = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                orderId = notification.OrderId,
                referenceType = notification.ReferenceType,
                referenceId = notification.ReferenceId,
                isRead = notification.IsRead,
                createdAt = notification.CreatedAt,
                aggregationCount = notification.AggregationCount,
                lastOccurrenceAt = notification.LastOccurrenceAt
            };

            foreach (var userId in userIds)
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("ReceiveNotification", payload);
            }
        }
        catch
        {
            // Non-critical
        }
    }
}
