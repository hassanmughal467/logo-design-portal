namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
