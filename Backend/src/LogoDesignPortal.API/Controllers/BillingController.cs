using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Billing;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the billing queue overview: clients with uninvoiced completed orders.
    /// </summary>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(List<BillingQueueOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingQueue([FromQuery] Guid? clientId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] bool onlyUninvoiced = true)
    {
        if (clientId.HasValue || fromDate.HasValue || toDate.HasValue || !onlyUninvoiced)
        {
            var filtered = await _billingService.GetBillingQueueAsync(new BillingQueueFilterDto
            {
                ClientId = clientId,
                FromDate = fromDate,
                ToDate = toDate,
                OnlyUninvoiced = onlyUninvoiced
            });
            return Ok(filtered);
        }

        var overview = await _billingService.GetBillingQueueOverviewAsync();
        return Ok(overview);
    }

    /// <summary>
    /// Gets eligible orders for a client (Completed, BillingEligible, not IsInvoiced).
    /// </summary>
    [HttpGet("clients/{clientId}/eligible-orders")]
    [ProducesResponseType(typeof(List<BillingEligibleOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEligibleOrders(Guid clientId)
    {
        var orders = await _billingService.GetEligibleOrdersForClientAsync(clientId);
        return Ok(orders);
    }

    /// <summary>
    /// Creates an invoice from selected orders for a client. Admin manual invoice generation.
    /// </summary>
    [HttpPost("clients/{clientId}/create-invoice")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoiceFromOrders(Guid clientId, [FromBody] CreateInvoiceFromOrdersRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var invoice = await _billingService.CreateInvoiceFromOrdersAsync(
                clientId,
                request.Orders,
                request.OrderIds,
                request.BillingPeriod,
                userId);
            return Created($"/api/invoices/{invoice.Id}", invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CreateInvoiceFromOrdersRequestDto
{
    /// <summary>Order IDs only (uses ClientChargePrice from order).</summary>
    public List<Guid> OrderIds { get; set; } = new();

    /// <summary>Orders with editable price per line. When provided, overrides OrderIds and uses these prices.</summary>
    public List<CreateInvoiceOrderItemDto>? Orders { get; set; }

    public string? BillingPeriod { get; set; }
}
