using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Monthly payout invoice for a designer, grouping completed orders with approved prices.
/// </summary>
public class DesignerInvoice : BaseEntity
{
    public Guid DesignerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DesignerInvoiceStatus Status { get; set; } = DesignerInvoiceStatus.Pending;
    /// <summary>Billing period (e.g. "March 2026").</summary>
    public string BillingPeriod { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public DesignerProfile Designer { get; set; } = null!;
    public ICollection<DesignerInvoiceItem> Items { get; set; } = new List<DesignerInvoiceItem>();
    public ICollection<DesignerInvoiceAdjustment> Adjustments { get; set; } = new List<DesignerInvoiceAdjustment>();
}
