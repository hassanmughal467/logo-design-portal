namespace LogoDesignPortal.Application.DTOs.Invoices;

public class CreateInvoiceRequestDto
{
    public Guid OrderId { get; set; }
    public decimal? TaxAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}
