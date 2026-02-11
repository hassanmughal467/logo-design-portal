using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class InvoiceLog : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public InvoiceAction Action { get; set; }
    public Guid? PerformedBy { get; set; } // Nullable for system actions
    public string? Notes { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
}
