using LogoDesignPortal.Application.DTOs.Invoices;

namespace LogoDesignPortal.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<List<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole);
    Task<List<InvoiceResponseDto>> GetInvoicesByClientAsync(Guid clientId);
    Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequestDto request, Guid updatedBy);
    Task<InvoiceResponseDto> MarkInvoiceAsPaidAsync(Guid invoiceId, string? paymentMethod, Guid? performedBy);
    Task<bool> SendInvoiceAsync(Guid invoiceId, Guid? performedBy);
    Task<List<InvoiceLogDto>> GetInvoiceLogsAsync(Guid invoiceId);
    Task UpdateOverdueInvoicesAsync(); // For background job
    Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(Guid? userId = null, string? userRole = null);
}
