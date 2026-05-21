using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Exceptions;
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
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var payment = await _paymentService.CreatePaymentAsync(request, userId, userRole);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var userId = User.GetUserIdOrThrow();
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var payment = await _paymentService.GetPaymentByIdWithAccessAsync(id, userId, userRole);
        if (payment == null)
        {
            return NotFound(new { error = "Payment not found." });
        }
        return Ok(payment);
    }

    [HttpGet("invoice/{invoiceId}")]
    [ProducesResponseType(typeof(List<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentsByInvoice(Guid invoiceId)
    {
        var userId = User.GetUserIdOrThrow();
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var payments = await _paymentService.GetPaymentsByInvoiceWithAccessAsync(invoiceId, userId, userRole);
        if (payments == null)
        {
            return NotFound(new { error = "Invoice not found." });
        }
        return Ok(payments);
    }

    [HttpPost("link")]
    [ProducesResponseType(typeof(PaymentLinkResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePaymentLink([FromBody] GeneratePaymentLinkRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var paymentLink = await _paymentService.GeneratePaymentLinkAsync(
                request.InvoiceId,
                request.PaymentMethod,
                userId,
                userRole);
            return Ok(paymentLink);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var payment = await _paymentService.ProcessPaymentAsync(request, userId, userRole);
            return Ok(payment);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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
    [Authorize(Roles = "Client,Admin,SuperAdmin")]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PayPalWebhook()
    {
        try
        {
            // Read raw body for signature verification (must be exact bytes)
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var webhookEventJson = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(webhookEventJson))
            {
                _logger.LogWarning("PayPal webhook received with empty body");
                return BadRequest(new { error = "Webhook body is required." });
            }

            // PayPal sends these headers (case-insensitive)
            var transmissionId = Request.Headers["Paypal-Transmission-Id"].FirstOrDefault();
            var transmissionTime = Request.Headers["Paypal-Transmission-Time"].FirstOrDefault();
            var transmissionSig = Request.Headers["Paypal-Transmission-Sig"].FirstOrDefault();
            var authAlgo = Request.Headers["Paypal-Auth-Algo"].FirstOrDefault() ?? "SHA256withRSA";
            var certUrl = Request.Headers["Paypal-Cert-Url"].FirstOrDefault();

            if (string.IsNullOrEmpty(transmissionId) || string.IsNullOrEmpty(transmissionTime) || string.IsNullOrEmpty(transmissionSig) || string.IsNullOrEmpty(certUrl))
            {
                _logger.LogWarning("PayPal webhook missing required verification headers");
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Invalid webhook: missing verification headers." });
            }

            var isValid = await _paymentService.VerifyPayPalWebhookSignatureAsync(
                transmissionId, transmissionTime, transmissionSig, authAlgo, certUrl, webhookEventJson);

            if (!isValid)
            {
                _logger.LogWarning("PayPal webhook signature verification failed");
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Webhook signature verification failed." });
            }

            // TODO: Process webhook events (e.g. PAYMENT.CAPTURE.COMPLETED) to update payment status
            _logger.LogInformation("PayPal webhook verified and received successfully");
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
