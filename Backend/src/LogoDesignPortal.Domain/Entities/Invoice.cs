using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid ClientId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public BillingType BillingType { get; set; } = BillingType.PerLogo; // Default for backward compatibility
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    /// <summary>Billing period for display (e.g. "March 2026", "Week of 2026-03-09").</summary>
    public string? BillingPeriod { get; set; }

    // Navigation properties
    public ClientProfile Client { get; set; } = null!;
    public ICollection<InvoiceOrder> InvoiceOrders { get; set; } = new List<InvoiceOrder>(); // Support multiple orders per invoice
    public ICollection<InvoiceLog> InvoiceLogs { get; set; } = new List<InvoiceLog>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

