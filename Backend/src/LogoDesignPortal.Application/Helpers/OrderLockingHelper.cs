using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Helper for order locking when an order reaches a terminal status.
/// Terminal orders are read-only for Client and Designer; Admin cannot modify content.
/// </summary>
public static class OrderLockingHelper
{
    /// <summary>
    /// Terminal statuses that make an order immutable.
    /// No modifications (files, comments, messages, revisions) are allowed.
    /// </summary>
    private static readonly HashSet<OrderStatus> TerminalStatuses =
    [
        OrderStatus.Completed,
        OrderStatus.FinalApproved,
        OrderStatus.Cancelled,
        OrderStatus.CancelledByUser,
        OrderStatus.CancelledByAdmin,
        OrderStatus.Refunded
    ];

    /// <summary>
    /// Returns true if the order status is terminal (locked).
    /// Locked orders cannot be modified by any role.
    /// </summary>
    public static bool IsOrderLocked(OrderStatus status)
    {
        return TerminalStatuses.Contains(status);
    }

    /// <summary>
    /// Error message returned when attempting to modify a locked order.
    /// </summary>
    public const string LockedOrderMessage = "Order is completed and cannot be modified.";
}
