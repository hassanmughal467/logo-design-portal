namespace LogoDesignPortal.Application.DTOs.Billing;

public class BillingQueueFilterDto
{
    public Guid? ClientId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool OnlyUninvoiced { get; set; } = true;
}
