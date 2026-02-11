namespace LogoDesignPortal.Domain.Entities;

public class InvoiceOrder : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid? OrderId { get; set; } // Nullable to support non-order items
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public LogoOrder? Order { get; set; } // Nullable to support non-order items
}
