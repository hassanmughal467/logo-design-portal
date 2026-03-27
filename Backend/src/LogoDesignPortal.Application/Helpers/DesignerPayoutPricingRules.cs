using System.Linq.Expressions;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Designer payout (PKR) is tracked via <see cref="PriceApprovalStatus"/> and approved amounts.
/// Do not use <see cref="LogoOrder.PriceApproved"/> for designer payout — that flag is reserved for
/// the client charge approval workflow (see <see cref="LogoOrder.PriceUpdatedByRole"/> after client accepts).
/// </summary>
public static class DesignerPayoutPricingRules
{
    /// <summary>True when designer payout is finalized (auto-approved or admin-approved) with a positive approved amount.</summary>
    public static bool HasFinalizedDesignerPayout(LogoOrder order)
    {
        var approvedAmount = order.DesignerApprovedPrice ?? order.ApprovedPrice;
        if (!approvedAmount.HasValue || approvedAmount.Value <= 0)
            return false;

        if (order.PriceApprovalStatus is PriceApprovalStatus.Approved or PriceApprovalStatus.AutoApproved)
            return true;

        // Legacy rows: PriceApproved was set when admin approved designer payout
        return order.PriceApproved
               && order.PriceApprovalStatus is not PriceApprovalStatus.PendingApproval
               && order.PriceApprovalStatus is not PriceApprovalStatus.Modified
               && order.PriceApprovalStatus is not PriceApprovalStatus.Rejected;
    }

    /// <summary>EF-translatable predicate for completed orders eligible for designer invoicing.</summary>
    public static readonly Expression<Func<LogoOrder, bool>> EligibleForDesignerInvoiceExpression = o =>
        (o.DesignerApprovedPrice ?? o.ApprovedPrice ?? 0) > 0 &&
        (
            o.PriceApprovalStatus == PriceApprovalStatus.Approved ||
            o.PriceApprovalStatus == PriceApprovalStatus.AutoApproved ||
            (o.PriceApproved &&
             o.PriceApprovalStatus != PriceApprovalStatus.PendingApproval &&
             o.PriceApprovalStatus != PriceApprovalStatus.Modified &&
             o.PriceApprovalStatus != PriceApprovalStatus.Rejected)
        );
}
