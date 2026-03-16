namespace LogoDesignPortal.Application.DTOs.Billing;

public class BillingEligibleOrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime? CompletedDate { get; set; }
}
