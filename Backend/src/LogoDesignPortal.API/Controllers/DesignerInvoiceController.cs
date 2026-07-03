using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/designer-invoice")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class DesignerInvoiceController : ControllerBase
{
    private readonly IDesignerPayoutService _designerPayoutService;
    private readonly ILogger<DesignerInvoiceController> _logger;

    public DesignerInvoiceController(IDesignerPayoutService designerPayoutService, ILogger<DesignerInvoiceController> logger)
    {
        _designerPayoutService = designerPayoutService;
        _logger = logger;
    }

    /// <summary>
    /// Gets payout summary: designers with pending payout, total eligible orders, total pending amount.
    /// </summary>
    [HttpGet("payout-summary")]
    [ProducesResponseType(typeof(DesignerPayoutSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayoutSummary()
    {
        var result = await _designerPayoutService.GetPayoutSummaryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets orders eligible for invoice generation for a designer and period. Preview only, does not create invoice.
    /// </summary>
    [HttpGet("eligible-orders")]
    [ProducesResponseType(typeof(List<OrderPricingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleOrders([FromQuery] Guid designerId, [FromQuery] int year, [FromQuery] int month)
    {
        var result = await _designerPayoutService.GetEligibleOrdersForPeriodAsync(designerId, year, month);
        return Ok(result);
    }

    /// <summary>
    /// Gets designers with completed work not yet invoiced. Grouped by DesignerId.
    /// </summary>
    [HttpGet("payout-overview")]
    [ProducesResponseType(typeof(List<DesignerPayoutOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignerPayoutOverview()
    {
        var result = await _designerPayoutService.GetDesignerPayoutOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Generates a monthly designer payout invoice for a designer.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(DesignerInvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateDesignerInvoice([FromQuery] Guid designerId, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            var result = await _designerPayoutService.GenerateDesignerInvoiceAsync(designerId, year, month, adminUserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets designer invoices for a designer.
    /// </summary>
    [HttpGet("designer/{designerId}")]
    [ProducesResponseType(typeof(List<DesignerInvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignerInvoices(Guid designerId)
    {
        var result = await _designerPayoutService.GetDesignerInvoicesAsync(designerId);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single designer invoice by id.
    /// </summary>
    [HttpGet("{invoiceId}")]
    [ProducesResponseType(typeof(DesignerInvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDesignerInvoice(Guid invoiceId)
    {
        var result = await _designerPayoutService.GetDesignerInvoiceByIdAsync(invoiceId);
        if (result == null)
        {
            return NotFound(new { error = "Designer invoice not found." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Marks a designer invoice as paid.
    /// </summary>
    [HttpPut("{invoiceId}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkDesignerInvoicePaid(Guid invoiceId)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            await _designerPayoutService.MarkDesignerInvoicePaidAsync(invoiceId, adminUserId);
            return Ok(new { message = "Invoice marked as paid." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an invoice item amount. Only for unpaid invoices.
    /// </summary>
    [HttpPut("{invoiceId}/items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInvoiceItem(Guid invoiceId, Guid itemId, [FromBody] UpdateDesignerInvoiceItemRequest request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            await _designerPayoutService.UpdateDesignerInvoiceItemAsync(invoiceId, itemId, request.Amount, adminUserId);
            return Ok(new { message = "Item updated." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adds an adjustment (bonus or deduction) to an invoice. Only for unpaid invoices.
    /// </summary>
    [HttpPost("{invoiceId}/adjustments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAdjustment(Guid invoiceId, [FromBody] AddDesignerInvoiceAdjustmentRequest request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            await _designerPayoutService.AddDesignerInvoiceAdjustmentAsync(invoiceId, request.Description, request.Amount, adminUserId);
            return Ok(new { message = "Adjustment added." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Removes an adjustment from an invoice. Only for unpaid invoices.
    /// </summary>
    [HttpDelete("{invoiceId}/adjustments/{adjustmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveAdjustment(Guid invoiceId, Guid adjustmentId)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            await _designerPayoutService.RemoveDesignerInvoiceAdjustmentAsync(invoiceId, adjustmentId, adminUserId);
            return Ok(new { message = "Adjustment removed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class UpdateDesignerInvoiceItemRequest
{
    public decimal Amount { get; set; }
}

public class AddDesignerInvoiceAdjustmentRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
