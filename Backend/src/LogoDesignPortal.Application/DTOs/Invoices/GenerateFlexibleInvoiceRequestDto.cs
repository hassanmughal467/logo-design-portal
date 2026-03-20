using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class GenerateFlexibleInvoiceRequestDto
{
    [Required]
    public Guid ClientId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<Guid>? SelectedOrderIds { get; set; }
    public bool IncludeUninvoicedOnly { get; set; } = true;
    public string? Notes { get; set; }
}
