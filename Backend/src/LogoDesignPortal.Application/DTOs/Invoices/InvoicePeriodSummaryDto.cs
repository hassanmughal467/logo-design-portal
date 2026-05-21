namespace LogoDesignPortal.Application.DTOs.Invoices;

/// <summary>Bucket totals for a date range (e.g. current calendar week/month in UTC).</summary>
public class InvoicePeriodSummaryDto
{
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}
