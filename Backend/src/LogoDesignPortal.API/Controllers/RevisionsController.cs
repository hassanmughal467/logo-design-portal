using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RevisionsController : ControllerBase
{
    private readonly IRevisionService _revisionService;
    private readonly ILogger<RevisionsController> _logger;

    public RevisionsController(IRevisionService revisionService, ILogger<RevisionsController> logger)
    {
        _revisionService = revisionService;
        _logger = logger;
    }

    [HttpPost("orders/{orderId}/request")]
    [Authorize(Roles = "Client")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(RevisionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestRevision(Guid orderId, [FromForm] RequestRevisionDto request)
    {
        try
        {
            var requestedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _revisionService.RequestRevisionAsync(orderId, request, requestedBy);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("orders/{orderId}/latest")]
    [Authorize(Roles = "Admin,SuperAdmin,Designer")]
    [ProducesResponseType(typeof(RevisionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestRevision(Guid orderId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var result = await _revisionService.GetLatestRevisionAsync(orderId, userId, userRole);
            
            if (result == null)
                return NotFound(new { error = "No revision found for this order." });
            
            return Ok(result);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpPost("orders/{orderId}/approve-logo")]
    [Authorize(Roles = "Client,Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveLogo(Guid orderId, [FromBody] ApproveLogoDto request)
    {
        try
        {
            var approvedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _revisionService.ApproveLogoAsync(orderId, request, approvedBy);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("orders/{orderId}/can-request")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CanRequestRevision(Guid orderId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var canRequest = await _revisionService.CanRequestRevisionAsync(orderId, userId, "Client");
            return Ok(new { canRequest });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("orders/{orderId}/can-approve")]
    [Authorize(Roles = "Client,Admin,SuperAdmin")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CanApproveLogo(Guid orderId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var canApprove = await _revisionService.CanApproveLogoAsync(orderId, userId, userRole ?? string.Empty);
            return Ok(new { canApprove });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
