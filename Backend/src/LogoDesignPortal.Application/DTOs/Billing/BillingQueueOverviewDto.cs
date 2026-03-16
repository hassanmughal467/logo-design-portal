namespace LogoDesignPortal.Application.DTOs.Billing;

public class BillingQueueOverviewDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int UninvoicedOrderCount { get; set; }
    public decimal TotalPendingAmount { get; set; }
}
