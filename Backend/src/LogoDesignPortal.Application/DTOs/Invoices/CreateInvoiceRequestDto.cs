using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class CreateInvoiceRequestDto
{
    // Backward compatibility: support single order
    public Guid? OrderId { get; set; }
    
    // New: support multiple orders
    public List<Guid>? OrderIds { get; set; }
    
    // New: support manual items (non-order items)
    public List<CreateInvoiceItemDto>? ManualItems { get; set; }
    
    public BillingType? BillingType { get; set; } // Defaults to PerLogo for backward compatibility
    public decimal? TaxAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class CreateInvoiceItemDto
{
    public Guid? OrderId { get; set; } // Nullable for manual items
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
