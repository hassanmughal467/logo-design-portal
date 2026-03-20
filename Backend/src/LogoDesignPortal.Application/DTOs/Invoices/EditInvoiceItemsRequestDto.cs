using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class EditInvoiceItemsRequestDto
{
    public List<Guid>? RemoveItemIds { get; set; }
    public List<AddManualInvoiceItemDto>? AddManualItems { get; set; }
}

public class AddManualInvoiceItemDto
{
    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative.")]
    public decimal Amount { get; set; }
}
