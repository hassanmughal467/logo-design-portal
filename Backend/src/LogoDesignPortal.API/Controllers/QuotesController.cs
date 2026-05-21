using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.Constants;
using LogoDesignPortal.Application.DTOs.Quotes;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [RequestSizeLimit(UploadLimits.MaxMultipartBytes)]
    public async Task<IActionResult> Create([FromForm] string quote, [FromForm] IFormFileCollection? files)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });

        var request = System.Text.Json.JsonSerializer.Deserialize<CreateQuoteRequestDto>(quote, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (request == null) return BadRequest(new { error = "Invalid quote payload." });
        var created = await _quoteService.CreateQuoteAsync(request, files?.ToArray() ?? Array.Empty<IFormFile>(), userId);
        return Ok(created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });
        var role = User.FindFirstValue(ClaimTypes.Role);
        var data = await _quoteService.GetQuotesAsync(userId, role, status);
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });
        var role = User.FindFirstValue(ClaimTypes.Role);
        var quote = await _quoteService.GetQuoteByIdAsync(id, userId, role);
        return quote == null ? NotFound(new { error = "Quote not found." }) : Ok(quote);
    }

    [HttpPost("{id}/respond")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondQuoteRequestDto request)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });
        var updated = await _quoteService.RespondToQuoteAsync(id, request, userId);
        return Ok(updated);
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Reject(Guid id)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });
        var updated = await _quoteService.RejectQuoteAsync(id, userId);
        return Ok(updated);
    }

    [HttpPost("{id}/convert-to-order")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> ConvertToOrder(Guid id, [FromBody] ConvertQuoteRequestDto? request)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized(new { error = "User identity could not be determined." });
        var updated = await _quoteService.ConvertToOrderAsync(id, userId, request?.OrderId);
        return Ok(updated);
    }
}
