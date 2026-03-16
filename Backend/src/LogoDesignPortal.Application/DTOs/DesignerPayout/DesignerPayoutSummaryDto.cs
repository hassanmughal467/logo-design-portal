namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Summary of pending designer payouts: designers count, total orders, total amount.
/// Criteria: Status=Completed, PriceApproved=true, ApprovedPrice>0, IsDesignerInvoiced=false.
/// </summary>
public class DesignerPayoutSummaryDto
{
    public int DesignersWithPendingPayout { get; set; }
    public int TotalEligibleOrders { get; set; }
    public decimal TotalPendingPayoutAmount { get; set; }
}
