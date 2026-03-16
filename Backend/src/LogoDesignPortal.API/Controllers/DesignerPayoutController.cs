using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/designer-payout")]
[Authorize]
public class DesignerPayoutController : ControllerBase
{
    private readonly IDesignerPayoutService _designerPayoutService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DesignerPayoutController> _logger;

    public DesignerPayoutController(IDesignerPayoutService designerPayoutService, IApplicationDbContext context, ILogger<DesignerPayoutController> logger)
    {
        _designerPayoutService = designerPayoutService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets default pricing info for design types (for UI dropdowns).
    /// When the user is a Designer, returns their DesignerLogoPricing; otherwise returns global DesignPricing.
    /// </summary>
    [HttpGet("pricing-info")]
    [ProducesResponseType(typeof(List<DesignerPricingInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignPricingInfo()
    {
        Guid? designerId = null;
        if (User.IsInRole("Designer"))
        {
            var userId = User.GetUserIdOrThrow();
            var designer = await GetDesignerProfileByUserId(userId);
            if (designer != null)
                designerId = designer.Id;
        }
        var result = await _designerPayoutService.GetDesignPricingInfoAsync(designerId);
        return Ok(result);
    }

    /// <summary>
    /// Designer proposes a complexity price for an order. Admin must approve via approve-price endpoint.
    /// </summary>
    [HttpPost("pricing/propose")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ProposeDesignerPrice([FromBody] ProposeDesignerPriceRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            await _designerPayoutService.ProposeDesignerPriceAsync(request.OrderId, request, userId);
            return Ok(new { message = "Price proposed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Designer submits design category, type, and proposed price for an order.
    /// Can be called separately or as part of final file upload.
    /// </summary>
    [HttpPost("orders/{orderId}/submit-pricing")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitDesignerPricing(Guid orderId, [FromBody] SubmitDesignerPricingRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            await _designerPayoutService.SubmitDesignerPricingAsync(orderId, request, userId);
            return Ok(new { message = "Pricing submitted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Admin approves, modifies, or rejects designer's proposed price.
    /// </summary>
    [HttpPut("orders/{orderId}/approve-price")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveDesignerPrice(Guid orderId, [FromBody] ApproveDesignerPriceRequestDto request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            await _designerPayoutService.ApproveDesignerPriceAsync(orderId, request, adminUserId);
            return Ok(new { message = "Price approval processed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets orders pending price approval (admin queue).
    /// </summary>
    [HttpGet("orders/pending-approval")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(List<OrderPricingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersPendingPriceApproval()
    {
        var result = await _designerPayoutService.GetOrdersPendingPriceApprovalAsync();
        return Ok(result);
    }

    /// <summary>
    /// Designer gets their own orders where price is pending admin approval.
    /// </summary>
    [HttpGet("me/orders-pending-approval")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(typeof(List<OrderPricingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrdersPendingApproval()
    {
        var userId = User.GetUserIdOrThrow();
        var designer = await GetDesignerProfileByUserId(userId);
        if (designer == null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Designer profile not found." });

        var result = await _designerPayoutService.GetDesignerOrdersPendingApprovalAsync(designer.Id);
        return Ok(result);
    }

    /// <summary>
    /// Designer gets their own payout-eligible completed orders. Admin can use designers/{id}/eligible-orders.
    /// </summary>
    [HttpGet("me/eligible-orders")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(typeof(List<OrderPricingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayoutEligibleOrders()
    {
        var userId = User.GetUserIdOrThrow();
        var designer = await GetDesignerProfileByUserId(userId);
        if (designer == null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Designer profile not found." });

        var result = await _designerPayoutService.GetDesignerPayoutEligibleOrdersAsync(designer.Id);
        return Ok(result);
    }

    /// <summary>
    /// Gets payout-eligible orders for a designer. Admin/SuperAdmin only.
    /// </summary>
    [HttpGet("designers/{designerId}/eligible-orders")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(List<OrderPricingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignerPayoutEligibleOrders(Guid designerId)
    {
        var result = await _designerPayoutService.GetDesignerPayoutEligibleOrdersAsync(designerId);
        return Ok(result);
    }

    /// <summary>
    /// Gets eligible orders for the Invoice Builder grid. Admin/SuperAdmin only.
    /// Returns OrderNumber, ClientName, PreviewImageUrl, sorted by CompletedDate ascending.
    /// </summary>
    [HttpGet("orders")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(List<DesignerPayoutEligibleOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersForInvoiceBuilder([FromQuery] Guid designerId)
    {
        var result = await _designerPayoutService.GetDesignerPayoutEligibleOrdersForBuilderAsync(designerId);
        return Ok(result);
    }

    /// <summary>
    /// Gets order preview for design modal (FinalFiles from ClientGallery).
    /// </summary>
    [HttpGet("order-preview/{orderId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(DesignerPayoutOrderPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderPreview(Guid orderId)
    {
        var result = await _designerPayoutService.GetOrderPreviewAsync(orderId);
        if (result == null)
            return NotFound(new { error = "Order not found." });
        return Ok(result);
    }

    /// <summary>
    /// Generates a designer invoice from the Invoice Builder. Admin selects orders and amounts.
    /// Uses transaction; validates each order; locks orders on success.
    /// </summary>
    [HttpPost("generate-invoice")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(DesignerInvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateDesignerInvoiceRequestDto request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            var result = await _designerPayoutService.GenerateInvoiceFromBuilderAsync(request, adminUserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Designer gets their own invoices.
    /// </summary>
    [HttpGet("me/invoices")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(typeof(List<DesignerInvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyInvoices()
    {
        var userId = User.GetUserIdOrThrow();
        var designer = await GetDesignerProfileByUserId(userId);
        if (designer == null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Designer profile not found." });

        var result = await _designerPayoutService.GetDesignerInvoicesAsync(designer.Id);
        return Ok(result);
    }

    /// <summary>
    /// Designer gets a single invoice by id (must belong to the designer).
    /// </summary>
    [HttpGet("me/invoices/{invoiceId}")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(typeof(DesignerInvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyInvoice(Guid invoiceId)
    {
        var userId = User.GetUserIdOrThrow();
        var designer = await GetDesignerProfileByUserId(userId);
        if (designer == null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Designer profile not found." });

        var result = await _designerPayoutService.GetDesignerInvoiceByIdAsync(invoiceId);
        if (result == null || result.DesignerId != designer.Id)
            return NotFound(new { error = "Invoice not found." });
        return Ok(result);
    }

    private async Task<LogoDesignPortal.Domain.Entities.DesignerProfile?> GetDesignerProfileByUserId(Guid userId)
    {
        return await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
    }
}
