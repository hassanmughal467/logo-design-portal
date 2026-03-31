namespace LogoDesignPortal.Application.DTOs.Billing;

public class BillingQueueResultDto
{
    public List<BillingEligibleOrderDto> Orders { get; set; } = new();
    public decimal TotalAmountPreview { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
