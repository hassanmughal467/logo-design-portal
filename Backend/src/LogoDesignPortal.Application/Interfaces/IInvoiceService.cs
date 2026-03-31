using LogoDesignPortal.Application.DTOs.Common;
using LogoDesignPortal.Application.DTOs.Invoices;

namespace LogoDesignPortal.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId);
    /// <summary>
    /// Gets an invoice by ID with access control. Returns null if not found or user lacks access.
    /// </summary>
    Task<InvoiceResponseDto?> GetInvoiceByIdWithAccessAsync(Guid invoiceId, Guid userId, string? userRole);
    Task<PagedResultDto<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole, InvoiceQueryFilterDto? filters = null, int page = 1, int pageSize = 50);
    /// <summary>Loads up to <paramref name="maxRows"/> invoices for PDF/report export (bounded to protect memory).</summary>
    Task<List<InvoiceResponseDto>> GetInvoicesForExportAsync(Guid? userId, string? userRole, InvoiceQueryFilterDto? filters = null, int maxRows = 500);
    Task<List<InvoiceResponseDto>> GetInvoicesByClientAsync(Guid clientId);
    Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequestDto request, Guid updatedBy);
    Task<InvoiceResponseDto> GenerateFlexibleInvoiceAsync(GenerateFlexibleInvoiceRequestDto request, Guid createdBy);
    Task<InvoiceResponseDto> EditInvoiceItemsAsync(Guid invoiceId, EditInvoiceItemsRequestDto request, Guid updatedBy);
    Task<InvoiceResponseDto> MarkInvoiceAsPaidAsync(Guid invoiceId, string? paymentMethod, Guid? performedBy);
    Task<bool> SendInvoiceAsync(Guid invoiceId, Guid? performedBy);
    Task<List<InvoiceLogDto>> GetInvoiceLogsAsync(Guid invoiceId);
    /// <summary>
    /// Gets invoice logs with access control. Returns null if user lacks access to the invoice.
    /// </summary>
    Task<List<InvoiceLogDto>?> GetInvoiceLogsWithAccessAsync(Guid invoiceId, Guid userId, string? userRole);
    Task UpdateOverdueInvoicesAsync(); // For background job
    Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(Guid? userId = null, string? userRole = null);
}
