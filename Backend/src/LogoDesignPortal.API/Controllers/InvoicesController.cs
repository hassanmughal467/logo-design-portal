using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InvoiceStatisticsDto = LogoDesignPortal.Application.DTOs.Invoices.InvoiceStatisticsDto;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoicePdfService _invoicePdfService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, IInvoicePdfService invoicePdfService, ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _invoicePdfService = invoicePdfService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var invoice = await _invoiceService.CreateInvoiceAsync(request, userId);
            return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var invoice = await _invoiceService.GetInvoiceByIdWithAccessAsync(id, userId, userRole);
        if (invoice == null)
        {
            return NotFound(new { error = "Invoice not found." });
        }
        return Ok(invoice);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var invoices = await _invoiceService.GetInvoicesAsync(userId, userRole);
        return Ok(invoices);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, userId);
            return Ok(invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/mark-paid")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] MarkInvoicePaidRequestDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var invoice = await _invoiceService.MarkInvoiceAsPaidAsync(id, request.PaymentMethod, userId);
            return Ok(invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _invoiceService.SendInvoiceAsync(id, userId);
            if (result)
            {
                return Ok(new { message = "Invoice sent successfully." });
            }
            return BadRequest(new { error = "Failed to send invoice." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/logs")]
    [ProducesResponseType(typeof(List<InvoiceLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceLogs(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var logs = await _invoiceService.GetInvoiceLogsWithAccessAsync(id, userId, userRole);
        if (logs == null)
        {
            return NotFound(new { error = "Invoice not found." });
        }
        return Ok(logs);
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(InvoiceStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var statistics = await _invoiceService.GetInvoiceStatisticsAsync(userId, userRole);
        return Ok(statistics);
    }

    [HttpGet("report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadReport([FromQuery] string? status = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var invoices = await _invoiceService.GetInvoicesAsync(userId, userRole);

            // Filter by status if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                invoices = invoices.Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var reportTitle = string.IsNullOrWhiteSpace(status)
                ? "Invoice Report — All"
                : $"Invoice Report — {status}";

            var pdfBytes = await _invoicePdfService.GenerateInvoiceReportPdfAsync(invoices, reportTitle);
            var fileName = $"Invoice_Report_{status ?? "All"}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice report PDF");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to generate invoice report PDF." });
        }
    }

    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var invoice = await _invoiceService.GetInvoiceByIdWithAccessAsync(id, userId, userRole);
        if (invoice == null)
        {
            return NotFound(new { error = "Invoice not found." });
        }

        try
        {
            var pdfBytes = await _invoicePdfService.GenerateInvoicePdfAsync(invoice);
            var fileName = $"{invoice.InvoiceNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to generate invoice PDF." });
        }
    }
}

public class MarkInvoicePaidRequestDto
{
    public string? PaymentMethod { get; set; }
}
