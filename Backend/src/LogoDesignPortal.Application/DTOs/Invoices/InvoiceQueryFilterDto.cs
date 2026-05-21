using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceQueryFilterDto
{
    public Guid? ClientId { get; set; }
    public DateTime? IssueDateFrom { get; set; }
    public DateTime? IssueDateTo { get; set; }
    public BillingType? BillingType { get; set; }
    public InvoiceStatus? Status { get; set; }

    /// <summary>When true, returns all invoices except paid (overrides <see cref="Status"/>).</summary>
    public bool ExcludePaid { get; set; }
}
