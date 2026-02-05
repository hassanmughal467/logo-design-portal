namespace LogoDesignPortal.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsPaid { get; set; } = false;
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
}
