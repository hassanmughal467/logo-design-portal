using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.Constants;
using LogoDesignPortal.Application.DTOs.Settings;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SettingsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPost("business")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBusinessSettings([FromBody] Dictionary<string, object> data)
    {
        var userId = User.GetUserIdOrThrow();
        await _settingsService.UpdateSettingsAsync("Business", data, userId);
        return Ok(new { message = "Business settings updated successfully." });
    }

    [HttpPost("brand")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBrandSettings([FromBody] Dictionary<string, object> data)
    {
        var userId = User.GetUserIdOrThrow();
        await _settingsService.UpdateSettingsAsync("Brand", data, userId);
        return Ok(new { message = "Brand settings updated successfully." });
    }

    [HttpPost("logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile logo)
    {
        if (logo == null || logo.Length == 0)
        {
            return BadRequest(new { error = "No logo file uploaded." });
        }

        if (logo.Length > UploadLimits.MaxSingleFileBytes)
        {
            return BadRequest(new { error = "Logo file exceeds maximum allowed size." });
        }

        try
        {
            UploadSecurityHelper.ValidateUploadFileName(logo.FileName);
            var ext = UploadSecurityHelper.GetEffectiveExtension(logo.FileName);
            if (ext is not ".png" and not ".jpg" and not ".jpeg" and not ".webp" and not ".svg")
            {
                return BadRequest(new { error = "Logo must be PNG, JPEG, WebP, or SVG." });
            }

            UploadSecurityHelper.ValidateDeclaredContentType(ext, logo.ContentType);
            using var stream = logo.OpenReadStream();
            UploadSecurityHelper.ValidateMagicBytes(ext, stream);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        // TODO: Implement logo upload to storage
        // For now, save the logo URL/path as a setting
        var userId = User.GetUserIdOrThrow();
        var logoUrl = $"/uploads/logo/{logo.FileName}"; // Placeholder

        var data = new Dictionary<string, object> { { "logoUrl", logoUrl } };
        await _settingsService.UpdateSettingsAsync("Brand", data, userId);

        return Ok(new { logoUrl, message = "Logo uploaded successfully." });
    }

    [HttpPost("invoice-template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvoiceTemplate([FromBody] Dictionary<string, object> data)
    {
        var userId = User.GetUserIdOrThrow();
        await _settingsService.UpdateSettingsAsync("InvoiceTemplate", data, userId);
        return Ok(new { message = "Invoice template updated successfully." });
    }

    [HttpPost("payment-methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePaymentMethods([FromBody] UpdatePaymentMethodsRequestDto request)
    {
        var userId = User.GetUserIdOrThrow();
        var data = new Dictionary<string, object>
        {
            { "paymentMethods", System.Text.Json.JsonSerializer.Serialize(request.PaymentMethods) }
        };
        await _settingsService.UpdateSettingsAsync("PaymentMethods", data, userId);
        return Ok(new { message = "Payment methods updated successfully." });
    }

    [HttpPost("invoice")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvoiceSettings([FromBody] Dictionary<string, object> data)
    {
        var userId = User.GetUserIdOrThrow();
        await _settingsService.UpdateSettingsAsync("Invoice", data, userId);
        return Ok(new { message = "Invoice settings updated successfully." });
    }

    [HttpPost("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNotifications([FromBody] Dictionary<string, bool> data)
    {
        var userId = User.GetUserIdOrThrow();
        var settingsData = data.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        await _settingsService.UpdateSettingsAsync("Notifications", settingsData, userId);
        return Ok(new { message = "Notification preferences updated successfully." });
    }
}

public class UpdatePaymentMethodsRequestDto
{
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
}
