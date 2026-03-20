namespace LogoDesignPortal.Application.DTOs.Billing;

public class BillingQueueResultDto
{
    public List<BillingEligibleOrderDto> Orders { get; set; } = new();
    public decimal TotalAmountPreview { get; set; }
}
