using LogoDesignPortal.Application.DTOs.Invoices;

namespace LogoDesignPortal.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<List<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole);
    Task<List<InvoiceResponseDto>> GetInvoicesByClientAsync(Guid clientId);
    Task<InvoiceResponseDto> MarkInvoiceAsPaidAsync(Guid invoiceId, string? paymentMethod);
    Task<bool> SendInvoiceAsync(Guid invoiceId);
}
