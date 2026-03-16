namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Line item in a designer invoice, linking a completed order with its approved payout amount.
/// </summary>
public class DesignerInvoiceItem : BaseEntity
{
    public Guid DesignerInvoiceId { get; set; }
    public Guid OrderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Navigation properties
    public DesignerInvoice DesignerInvoice { get; set; } = null!;
    public LogoOrder Order { get; set; } = null!;
}
