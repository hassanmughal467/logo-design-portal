using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.ClientLogoPricing;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/client-logo-pricing")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ClientLogoPricingController : ControllerBase
{
    private readonly IClientLogoPricingService _clientLogoPricingService;
    private readonly ILogger<ClientLogoPricingController> _logger;

    public ClientLogoPricingController(IClientLogoPricingService clientLogoPricingService, ILogger<ClientLogoPricingController> logger)
    {
        _clientLogoPricingService = clientLogoPricingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pricing for a client.
    /// </summary>
    [HttpGet("client/{clientId}")]
    [ProducesResponseType(typeof(List<ClientLogoPricingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClientId(Guid clientId)
    {
        var result = await _clientLogoPricingService.GetByClientIdAsync(clientId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single pricing by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientLogoPricingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _clientLogoPricingService.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Create or update pricing for a client (upsert by ClientId + DesignCategory + DesignType).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientLogoPricingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientLogoPricingRequestDto request)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var result = await _clientLogoPricingService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update existing pricing.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClientLogoPricingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientLogoPricingRequestDto request)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        try
        {
            var result = await _clientLogoPricingService.UpdateAsync(id, request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Disable (soft delete) pricing.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var deleted = await _clientLogoPricingService.DeleteAsync(id, userId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
