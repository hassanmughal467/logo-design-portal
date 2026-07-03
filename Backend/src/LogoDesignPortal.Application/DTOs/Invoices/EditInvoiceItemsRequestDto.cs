using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class EditInvoiceItemsRequestDto
{
    public List<Guid>? RemoveItemIds { get; set; }

    /// <summary>Completed, billing-eligible orders to attach to this unpaid invoice (sets IsInvoiced on each order).</summary>
    public List<Guid>? AddOrderIds { get; set; }

    public List<AddManualInvoiceItemDto>? AddManualItems { get; set; }
}

public class AddManualInvoiceItemDto
{
    /// <summary>When set, links the line to an order and marks that order invoiced (excludes it from new invoice generation).</summary>
    public Guid? OrderId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative.")]
    public decimal Amount { get; set; }
}
