namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Sends real-time entity update events to connected clients via SignalR.
/// Used for data grid synchronization - separate from user-facing notifications.
/// </summary>
public interface IRealtimeEntityUpdateSender
{
    /// <summary>
    /// Notifies when a new order is created. Sent to Admin/SuperAdmin.
    /// </summary>
    Task SendOrderCreatedAsync(Guid orderId, IEnumerable<Guid> userIds);

    /// <summary>
    /// Notifies when admin assigns a designer to an order. Sent to Designer.
    /// </summary>
    Task SendOrderAssignedAsync(Guid orderId, Guid designerUserId);

    /// <summary>
    /// Notifies when designer uploads preview files. Sent to Admin/SuperAdmin.
    /// </summary>
    Task SendPreviewUploadedAsync(Guid orderId, IEnumerable<Guid> adminUserIds);

    /// <summary>
    /// Notifies when order status changes (e.g. admin marks Completed).
    /// </summary>
    Task SendOrderStatusChangedAsync(Guid orderId, string status, Guid? updatedBy, IEnumerable<Guid> userIds);

    /// <summary>
    /// Notifies when client approves preview (FinalApproved). Sent to Admin.
    /// </summary>
    Task SendPreviewApprovedAsync(Guid orderId, string clientName, string status, IEnumerable<Guid> userIds);

    /// <summary>
    /// Notifies when client rejects preview (RevisionRequested). Sent to Admin/Designer.
    /// </summary>
    Task SendPreviewRejectedAsync(Guid orderId, string clientName, IEnumerable<Guid> userIds);

    /// <summary>
    /// Notifies when admin delivers preview files to client.
    /// </summary>
    Task SendPreviewDeliveredAsync(Guid orderId, string status, Guid clientUserId);

    /// <summary>
    /// Notifies when order is updated (details changed).
    /// </summary>
    Task SendOrderUpdatedAsync(Guid orderId, string status, IEnumerable<Guid> userIds);

    /// <summary>
    /// Notifies when invoice is generated for an order.
    /// </summary>
    Task SendInvoiceGeneratedAsync(Guid orderId, Guid invoiceId, Guid clientUserId);
}
