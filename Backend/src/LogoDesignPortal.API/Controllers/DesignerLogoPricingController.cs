using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.DesignerLogoPricing;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/designer-logo-pricing")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class DesignerLogoPricingController : ControllerBase
{
    private readonly IDesignerLogoPricingService _designerLogoPricingService;
    private readonly ILogger<DesignerLogoPricingController> _logger;

    public DesignerLogoPricingController(IDesignerLogoPricingService designerLogoPricingService, ILogger<DesignerLogoPricingController> logger)
    {
        _designerLogoPricingService = designerLogoPricingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pricing for a designer.
    /// </summary>
    [HttpGet("designer/{designerId}")]
    [ProducesResponseType(typeof(List<DesignerLogoPricingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDesignerId(Guid designerId)
    {
        var result = await _designerLogoPricingService.GetByDesignerIdAsync(designerId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single pricing by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DesignerLogoPricingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _designerLogoPricingService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create or update pricing for a designer (upsert by DesignerId + DesignCategory + DesignType).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DesignerLogoPricingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDesignerLogoPricingRequestDto request)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _designerLogoPricingService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update existing pricing.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DesignerLogoPricingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDesignerLogoPricingRequestDto request)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _designerLogoPricingService.UpdateAsync(id, request, userId);
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
        {
            return Unauthorized();
        }

        var deleted = await _designerLogoPricingService.DeleteAsync(id, userId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
