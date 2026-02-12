using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var payment = await _paymentService.CreatePaymentAsync(request, userId);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to create payment" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { error = "Payment not found." });
        }
        return Ok(payment);
    }

    [HttpGet("invoice/{invoiceId}")]
    [ProducesResponseType(typeof(List<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByInvoice(Guid invoiceId)
    {
        var payments = await _paymentService.GetPaymentsByInvoiceAsync(invoiceId);
        return Ok(payments);
    }

    [HttpPost("link")]
    [ProducesResponseType(typeof(PaymentLinkResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePaymentLink([FromBody] GeneratePaymentLinkRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var paymentLink = await _paymentService.GeneratePaymentLinkAsync(
                request.InvoiceId, 
                request.PaymentMethod, 
                userId);
            return Ok(paymentLink);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment link");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to generate payment link" });
        }
    }

    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var payment = await _paymentService.ProcessPaymentAsync(request, userId);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to process payment" });
        }
    }

    [HttpGet("bank-details")]
    [ProducesResponseType(typeof(BankDetailsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBankDetails()
    {
        var bankDetails = await _paymentService.GetBankDetailsAsync();
        return Ok(bankDetails);
    }

    [HttpPost("verify/paypal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyPayPalPayment([FromBody] VerifyPayPalRequestDto request)
    {
        try
        {
            var verified = await _paymentService.VerifyPayPalPaymentAsync(request.OrderId, request.PaymentId);
            return Ok(new { verified });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PayPal payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to verify payment" });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePaymentStatus(
        Guid id, 
        [FromBody] UpdatePaymentStatusRequestDto request)
    {
        try
        {
            var payment = await _paymentService.UpdatePaymentStatusAsync(
                id, 
                request.Status, 
                request.TransactionId);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to update payment status" });
        }
    }

    [HttpPost("webhook/paypal")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PayPalWebhook([FromBody] object webhookData)
    {
        try
        {
            // Handle PayPal webhook
            // This would verify the webhook signature and process the payment update
            _logger.LogInformation("PayPal webhook received");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal webhook");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

public class GeneratePaymentLinkRequestDto
{
    public Guid InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class VerifyPayPalRequestDto
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
}

public class UpdatePaymentStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}
