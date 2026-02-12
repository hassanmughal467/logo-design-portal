using LogoDesignPortal.Application.DTOs.Invoices;

namespace LogoDesignPortal.Application.Interfaces;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(InvoiceResponseDto invoice);
    Task<byte[]> GenerateInvoiceReportPdfAsync(List<InvoiceResponseDto> invoices, string reportTitle = "Invoice Report");
}
