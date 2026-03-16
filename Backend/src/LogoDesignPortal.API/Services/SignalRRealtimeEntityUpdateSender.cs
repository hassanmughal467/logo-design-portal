using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LogoDesignPortal.API.Services;

public class SignalRRealtimeEntityUpdateSender : IRealtimeEntityUpdateSender
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;
    private readonly ILogger<SignalRRealtimeEntityUpdateSender> _logger;

    public SignalRRealtimeEntityUpdateSender(IHubContext<Hubs.NotificationHub> hubContext, ILogger<SignalRRealtimeEntityUpdateSender> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendOrderCreatedAsync(Guid orderId, IEnumerable<Guid> userIds)
    {
        try
        {
            var payload = new { orderId };
            foreach (var userId in userIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("OrderCreated", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR OrderCreated failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendOrderAssignedAsync(Guid orderId, Guid designerUserId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user-{designerUserId}")
                .SendAsync("OrderAssigned", new { orderId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR OrderAssigned failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendPreviewUploadedAsync(Guid orderId, IEnumerable<Guid> adminUserIds)
    {
        try
        {
            var payload = new { orderId };
            foreach (var userId in adminUserIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("PreviewUploaded", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR PreviewUploaded failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendOrderStatusChangedAsync(Guid orderId, string status, Guid? updatedBy, IEnumerable<Guid> userIds)
    {
        try
        {
            var payload = new { orderId, status, updatedBy };
            foreach (var userId in userIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("OrderStatusChanged", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR OrderStatusChanged failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendPreviewApprovedAsync(Guid orderId, string clientName, string status, IEnumerable<Guid> userIds)
    {
        try
        {
            var payload = new { orderId, clientName, status };
            foreach (var userId in userIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("PreviewApproved", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR PreviewApproved failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendPreviewRejectedAsync(Guid orderId, string clientName, IEnumerable<Guid> userIds)
    {
        try
        {
            var payload = new { orderId, clientName };
            foreach (var userId in userIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("PreviewRejected", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR PreviewRejected failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendPreviewDeliveredAsync(Guid orderId, string status, Guid clientUserId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user-{clientUserId}")
                .SendAsync("PreviewDelivered", new { orderId, status });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR PreviewDelivered failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendOrderUpdatedAsync(Guid orderId, string status, IEnumerable<Guid> userIds)
    {
        try
        {
            var payload = new { orderId, status };
            foreach (var userId in userIds.Distinct())
            {
                await _hubContext.Clients
                    .Group($"user-{userId}")
                    .SendAsync("OrderUpdated", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR OrderUpdated failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }

    public async Task SendInvoiceGeneratedAsync(Guid orderId, Guid invoiceId, Guid clientUserId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user-{clientUserId}")
                .SendAsync("InvoiceGenerated", new { orderId, invoiceId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR InvoiceGenerated failed: OrderId={OrderId}. Operation continues.", orderId);
        }
    }
}
