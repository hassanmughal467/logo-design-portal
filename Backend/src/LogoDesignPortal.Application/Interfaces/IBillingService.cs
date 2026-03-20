using LogoDesignPortal.Application.DTOs.Billing;
using LogoDesignPortal.Application.DTOs.Invoices;

namespace LogoDesignPortal.Application.Interfaces;

public interface IBillingService
{
    /// <summary>
    /// Gets clients with uninvoiced completed orders for the billing dashboard.
    /// </summary>
    Task<List<BillingQueueOverviewDto>> GetBillingQueueOverviewAsync();
    Task<BillingQueueResultDto> GetBillingQueueAsync(BillingQueueFilterDto filter);

    /// <summary>
    /// Gets all eligible (Completed, BillingEligible, not IsInvoiced) orders for a client.
    /// </summary>
    Task<List<BillingEligibleOrderDto>> GetEligibleOrdersForClientAsync(Guid clientId);

    /// <summary>
    /// Creates an invoice from selected orders for a client. Admin manual invoice generation.
    /// </summary>
    /// <summary>
    /// Creates invoice from orders. When orders is provided, uses editable prices. Otherwise uses orderIds and fetches ClientChargePrice from each order.
    /// </summary>
    Task<InvoiceResponseDto> CreateInvoiceFromOrdersAsync(Guid clientId, List<CreateInvoiceOrderItemDto>? orders, List<Guid>? orderIds, string? billingPeriod, Guid createdBy);

    /// <summary>
    /// Processes automatic invoice generation for Weekly and Monthly clients. Called by background job.
    /// </summary>
    Task ProcessAutomaticInvoicingAsync(Guid? systemUserId = null);
}
