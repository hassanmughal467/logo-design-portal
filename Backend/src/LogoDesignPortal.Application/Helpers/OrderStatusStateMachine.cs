using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Defines valid order status transitions. Rejects invalid transitions.
/// </summary>
public static class OrderStatusStateMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.WaitingForAdminApproval] = new() { OrderStatus.InProgress, OrderStatus.PriceApprovalPending, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.PriceApprovalPending] = new() { OrderStatus.InProgress, OrderStatus.WaitingForAdminApproval, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.InProgress] = new() { OrderStatus.PreviewDelivered, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.PreviewDelivered] = new() { OrderStatus.RevisionRequested, OrderStatus.ClientApproved, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.RevisionRequested] = new() { OrderStatus.PreviewDelivered, OrderStatus.InProgress, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.ClientApproved] = new() { OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.Completed] = new() { OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new(),
        [OrderStatus.CancelledByUser] = new(),
        [OrderStatus.CancelledByAdmin] = new(),
        [OrderStatus.Refunded] = new()
    };

    /// <summary>
    /// Validates that transitioning from currentStatus to newStatus is allowed.
    /// Throws InvalidOperationException if transition is invalid.
    /// </summary>
    public static void ValidateTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus == newStatus)
            return;
        if (AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus))
            return;
        throw new InvalidOperationException($"Invalid order status transition from {currentStatus} to {newStatus}.");
    }
}
