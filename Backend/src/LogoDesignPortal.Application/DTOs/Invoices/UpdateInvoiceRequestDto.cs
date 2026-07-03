using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class UpdateInvoiceRequestDto
{
    public BillingType? BillingType { get; set; }
    public decimal? TaxAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    // Allow updating individual item amounts/descriptions
    public List<UpdateInvoiceItemDto>? Items { get; set; }
}

public class UpdateInvoiceItemDto
{
    public Guid Id { get; set; } // Existing item ID
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
}
