using LogoDesignPortal.Domain;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Centralizes order status changes with state-machine validation.
/// </summary>
public static class OrderStatusTransitionHelper
{
    /// <summary>Validates against <see cref="OrderStatusStateMachine"/> then assigns status.</summary>
    public static void Apply(LogoOrder order, OrderStatus newStatus)
    {
        OrderStatusStateMachine.ValidateTransition(order.Status, newStatus);
        order.Status = newStatus;
    }

    /// <summary>Admin cancellation may occur from any non-terminal workflow state.</summary>
    public static void ApplyAdminCancellation(LogoOrder order)
    {
        if (order.Status is OrderStatus.Cancelled or OrderStatus.CancelledByUser or OrderStatus.CancelledByAdmin or OrderStatus.Refunded)
            throw new InvalidOperationException($"Order is already in terminal status {order.Status}.");

        order.Status = OrderStatus.CancelledByAdmin;
    }

    /// <summary>Client cancellation (only from early states per business rules).</summary>
    public static void ApplyClientCancellation(LogoOrder order)
    {
        OrderStatusStateMachine.ValidateTransition(order.Status, OrderStatus.CancelledByUser);
        order.Status = OrderStatus.CancelledByUser;
    }

    /// <summary>Backfill / manual completed orders may jump to Completed without a normal preview path.</summary>
    public static void ApplyManualCompleted(LogoOrder order)
    {
        if (order.Status is OrderStatus.Completed or OrderStatus.Refunded or OrderStatus.Cancelled
            or OrderStatus.CancelledByUser or OrderStatus.CancelledByAdmin)
            throw new InvalidOperationException($"Cannot mark order as completed from {order.Status}.");

        order.Status = OrderStatus.Completed;
    }
}
