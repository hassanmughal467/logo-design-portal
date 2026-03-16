using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface IDesignerPayoutService
{
    /// <summary>
    /// Gets default pricing info for design types (for UI dropdowns).
    /// When designerId is provided, checks DesignerLogoPricing first, then falls back to global DesignPricing.
    /// </summary>
    Task<List<DesignerPricingInfoDto>> GetDesignPricingInfoAsync(Guid? designerId = null);

    /// <summary>
    /// Designer proposes a complexity price for an order. Saves DesignerProposedPrice. Admin must approve.
    /// </summary>
    Task ProposeDesignerPriceAsync(Guid orderId, ProposeDesignerPriceRequestDto request, Guid designerUserId);

    /// <summary>
    /// Submits designer's proposed price for an order. Called when designer uploads final files.
    /// If price differs from standard, sets RequiresPriceApproval and PriceApprovalStatus.PendingApproval.
    /// </summary>
    Task SubmitDesignerPricingAsync(Guid orderId, SubmitDesignerPricingRequestDto request, Guid designerUserId);

    /// <summary>
    /// Admin approves, modifies, or rejects designer's proposed price. Only Admin/SuperAdmin.
    /// </summary>
    Task ApproveDesignerPriceAsync(Guid orderId, ApproveDesignerPriceRequestDto request, Guid adminUserId);

    /// <summary>
    /// Gets orders pending price approval (for admin queue).
    /// </summary>
    Task<List<OrderPricingSummaryDto>> GetOrdersPendingPriceApprovalAsync();

    /// <summary>
    /// Generates a monthly designer invoice for a designer. Only Admin/SuperAdmin.
    /// Includes all completed orders with approved prices that are not yet designer-invoiced.
    /// </summary>
    Task<DesignerInvoiceResponseDto> GenerateDesignerInvoiceAsync(Guid designerId, int year, int month, Guid adminUserId);

    /// <summary>
    /// Gets designer invoices for a designer.
    /// </summary>
    Task<List<DesignerInvoiceResponseDto>> GetDesignerInvoicesAsync(Guid designerId);

    /// <summary>
    /// Gets a single designer invoice by id.
    /// </summary>
    Task<DesignerInvoiceResponseDto?> GetDesignerInvoiceByIdAsync(Guid invoiceId);

    /// <summary>
    /// Marks designer invoice as paid. Only Admin/SuperAdmin.
    /// </summary>
    Task MarkDesignerInvoicePaidAsync(Guid invoiceId, Guid adminUserId);

    /// <summary>
    /// Gets completed orders eligible for designer payout (approved price, not yet designer-invoiced) for a designer.
    /// </summary>
    Task<List<OrderPricingSummaryDto>> GetDesignerPayoutEligibleOrdersAsync(Guid designerId);

    /// <summary>
    /// Gets eligible orders for the Invoice Builder grid. Same criteria as GetDesignerPayoutEligibleOrdersAsync
    /// but returns DesignerPayoutEligibleOrderDto with OrderNumber, ClientName, PreviewImageUrl. Sorted by CompletedDate ascending.
    /// </summary>
    Task<List<DesignerPayoutEligibleOrderDto>> GetDesignerPayoutEligibleOrdersForBuilderAsync(Guid designerId);

    /// <summary>
    /// Gets order preview for design modal (FinalFiles from ClientGallery).
    /// </summary>
    Task<DesignerPayoutOrderPreviewDto?> GetOrderPreviewAsync(Guid orderId);

    /// <summary>
    /// Generates a designer invoice from the Invoice Builder. Admin selects orders and amounts. Uses transaction.
    /// </summary>
    Task<DesignerInvoiceResponseDto> GenerateInvoiceFromBuilderAsync(GenerateDesignerInvoiceRequestDto request, Guid adminUserId);

    /// <summary>
    /// Gets designer's own orders where price is pending admin approval.
    /// </summary>
    Task<List<OrderPricingSummaryDto>> GetDesignerOrdersPendingApprovalAsync(Guid designerId);

    /// <summary>
    /// Admin updates an invoice item amount. Only for unpaid invoices.
    /// </summary>
    Task UpdateDesignerInvoiceItemAsync(Guid invoiceId, Guid itemId, decimal amount, Guid adminUserId);

    /// <summary>
    /// Admin adds an adjustment (bonus or deduction) to an invoice. Only for unpaid invoices.
    /// </summary>
    Task AddDesignerInvoiceAdjustmentAsync(Guid invoiceId, string description, decimal amount, Guid adminUserId);

    /// <summary>
    /// Admin removes an adjustment from an invoice. Only for unpaid invoices.
    /// </summary>
    Task RemoveDesignerInvoiceAdjustmentAsync(Guid invoiceId, Guid adjustmentId, Guid adminUserId);

    /// <summary>
    /// Gets designers with completed work not yet invoiced. Orders: Status=Completed, PriceApproved=true, ApprovedPrice>0, IsDesignerInvoiced=false.
    /// Grouped by DesignerId.
    /// </summary>
    Task<List<DesignerPayoutOverviewDto>> GetDesignerPayoutOverviewAsync();

    /// <summary>
    /// Gets orders eligible for designer invoice for a given designer and period (year/month).
    /// Same criteria as GenerateDesignerInvoice but read-only. Used for preview before generation.
    /// </summary>
    Task<List<OrderPricingSummaryDto>> GetEligibleOrdersForPeriodAsync(Guid designerId, int year, int month);

    /// <summary>
    /// Gets payout summary: designers with pending payout, total eligible orders, total pending amount.
    /// Criteria: Status=Completed, PriceApproved=true, ApprovedPrice>0, IsDesignerInvoiced=false.
    /// </summary>
    Task<DesignerPayoutSummaryDto> GetPayoutSummaryAsync();
}

public class OrderPricingSummaryDto
{
    public Guid OrderId { get; set; }
    public string OrderTitle { get; set; } = string.Empty;
    public string? DesignCategory { get; set; }
    public string? DesignType { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? ProposedPrice { get; set; }
    public decimal? ApprovedPrice { get; set; }
    public string PriceApprovalStatus { get; set; } = string.Empty;
    public bool PriceApproved { get; set; }
    public DateTime? CompletedDate { get; set; }
}
