using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class CreateInvoiceRequestDto
{
    // Backward compatibility: support single order
    public Guid? OrderId { get; set; }

    // New: support multiple orders
    public List<Guid>? OrderIds { get; set; }

    /// <summary>Orders with editable price per line item. When provided, overrides OrderIds and uses these prices for InvoiceOrder.Amount.</summary>
    public List<CreateInvoiceOrderItemDto>? Orders { get; set; }

    // New: support manual items (non-order items)
    public List<CreateInvoiceItemDto>? ManualItems { get; set; }

    public BillingType? BillingType { get; set; } // Defaults to PerLogo for backward compatibility
    public string? BillingPeriod { get; set; } // e.g. "March 2026", "Week of 2026-03-09"
    public decimal? TaxAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class CreateInvoiceItemDto
{
    public Guid? OrderId { get; set; } // Nullable for manual items
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative.")]
    public decimal Amount { get; set; }
}

/// <summary>Order with editable price for invoice creation. Admin can adjust price per line before finalizing.</summary>
public class CreateInvoiceOrderItemDto
{
    public Guid OrderId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal Price { get; set; }
}
