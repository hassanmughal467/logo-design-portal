namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderTitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
