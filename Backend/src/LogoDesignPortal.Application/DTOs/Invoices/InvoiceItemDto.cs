namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderTitle { get; set; }
    // Order creation date (used for invoice line item display)
    public DateTime? OrderDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
