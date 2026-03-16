namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Request DTO for generating a designer invoice from the Invoice Builder.
/// </summary>
public class GenerateDesignerInvoiceRequestDto
{
    public Guid DesignerId { get; set; }
    public DateOnly BillingPeriodStart { get; set; }
    public DateOnly BillingPeriodEnd { get; set; }
    public List<GenerateDesignerInvoiceOrderDto> Orders { get; set; } = new();
}

public class GenerateDesignerInvoiceOrderDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}
