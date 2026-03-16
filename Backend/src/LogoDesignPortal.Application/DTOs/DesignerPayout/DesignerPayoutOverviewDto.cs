namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Designer-level summary of completed orders with approved prices that are not yet designer-invoiced.
/// </summary>
public class DesignerPayoutOverviewDto
{
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public int CompletedOrders { get; set; }
    public decimal PendingPayoutAmount { get; set; }
}
