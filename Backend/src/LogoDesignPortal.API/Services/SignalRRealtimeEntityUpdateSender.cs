using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LogoDesignPortal.API.Services;

public class SignalRRealtimeEntityUpdateSender : IRealtimeEntityUpdateSender
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;

    public SignalRRealtimeEntityUpdateSender(IHubContext<Hubs.NotificationHub> hubContext)
    {
        _hubContext = hubContext;
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
        catch
        {
            // Non-critical
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
        catch
        {
            // Non-critical - data sync failure must not affect business logic
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
        catch
        {
            // Non-critical
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
        catch
        {
            // Non-critical
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
        catch
        {
            // Non-critical
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
        catch
        {
            // Non-critical
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
        catch
        {
            // Non-critical
        }
    }
}
