using LogoDesignPortal.Application.DTOs.Settings;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _settingsService.UpdateSettingsAsync("Business", data, userId);
        return Ok(new { message = "Business settings updated successfully." });
    }

    [HttpPost("brand")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBrandSettings([FromBody] Dictionary<string, object> data)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

        // TODO: Implement logo upload to storage
        // For now, save the logo URL/path as a setting
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var logoUrl = $"/uploads/logo/{logo.FileName}"; // Placeholder
        
        var data = new Dictionary<string, object> { { "logoUrl", logoUrl } };
        await _settingsService.UpdateSettingsAsync("Brand", data, userId);

        return Ok(new { logoUrl, message = "Logo uploaded successfully." });
    }

    [HttpPost("invoice-template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvoiceTemplate([FromBody] Dictionary<string, object> data)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _settingsService.UpdateSettingsAsync("InvoiceTemplate", data, userId);
        return Ok(new { message = "Invoice template updated successfully." });
    }

    [HttpPost("payment-methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePaymentMethods([FromBody] UpdatePaymentMethodsRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var data = new Dictionary<string, object> 
        { 
            { "paymentMethods", System.Text.Json.JsonSerializer.Serialize(request.PaymentMethods) } 
        };
        await _settingsService.UpdateSettingsAsync("PaymentMethods", data, userId);
        return Ok(new { message = "Payment methods updated successfully." });
    }

    [HttpPost("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNotifications([FromBody] Dictionary<string, bool> data)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var settingsData = data.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        await _settingsService.UpdateSettingsAsync("Notifications", settingsData, userId);
        return Ok(new { message = "Notification preferences updated successfully." });
    }
}

public class UpdatePaymentMethodsRequestDto
{
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
}
