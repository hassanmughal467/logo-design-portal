using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain;

/// <summary>
/// Domain rules for valid order status transitions. Used by application services and covered by domain tests.
/// </summary>
public static class OrderStatusStateMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.WaitingForAdminApproval] = new() { OrderStatus.InProgress, OrderStatus.PriceApprovalPending, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin, OrderStatus.CancelledByUser },
        [OrderStatus.PriceApprovalPending] = new() { OrderStatus.InProgress, OrderStatus.WaitingForAdminApproval, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin, OrderStatus.CancelledByUser },
        [OrderStatus.InProgress] = new() { OrderStatus.PreviewDelivered, OrderStatus.PriceApprovalPending, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.PreviewDelivered] = new() { OrderStatus.RevisionRequested, OrderStatus.ClientApproved, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.RevisionRequested] = new() { OrderStatus.PreviewDelivered, OrderStatus.InProgress, OrderStatus.PriceApprovalPending, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.ClientApproved] = new() { OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.CancelledByAdmin },
        [OrderStatus.Completed] = new() { OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new(),
        [OrderStatus.CancelledByUser] = new(),
        [OrderStatus.CancelledByAdmin] = new(),
        [OrderStatus.Refunded] = new()
    };

    /// <summary>True when transitioning from <paramref name="current"/> to <paramref name="next"/> is allowed (same state is allowed).</summary>
    public static bool IsTransitionAllowed(OrderStatus current, OrderStatus next)
    {
        if (current == next)
            return true;
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
    }

    /// <summary>Validates transition; throws when invalid (excluding no-op same status).</summary>
    public static void ValidateTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus == newStatus)
            return;
        if (AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus))
            return;
        throw new InvalidOperationException($"Invalid order status transition from {currentStatus} to {newStatus}.");
    }
}
