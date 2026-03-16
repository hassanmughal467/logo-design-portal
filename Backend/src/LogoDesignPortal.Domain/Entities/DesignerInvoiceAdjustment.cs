namespace LogoDesignPortal.Domain.Entities;

/// <summary>
/// Manual adjustment (bonus or deduction) on a designer invoice. Admin/SuperAdmin can add before marking paid.
/// </summary>
public class DesignerInvoiceAdjustment : BaseEntity
{
    public Guid DesignerInvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    /// <summary>Amount in PKR. Positive = bonus, negative = deduction.</summary>
    public decimal Amount { get; set; }

    // Navigation properties
    public DesignerInvoice DesignerInvoice { get; set; } = null!;
}
