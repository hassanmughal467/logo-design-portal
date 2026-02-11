namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceStatisticsDto
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int DueInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal OverdueAmount { get; set; }
}
