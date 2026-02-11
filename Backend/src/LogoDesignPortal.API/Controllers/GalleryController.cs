using LogoDesignPortal.Application.DTOs.Gallery;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(
        IGalleryService galleryService,
        ILogger<GalleryController> logger)
    {
        _galleryService = galleryService;
        _logger = logger;
    }

    [HttpGet("my-gallery")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(List<GalleryItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyGallery()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var galleryItems = await _galleryService.GetClientGalleryAsync(userId);
            return Ok(galleryItems);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(GalleryItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGalleryItemById(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var galleryItem = await _galleryService.GetGalleryItemByIdAsync(id, userId);
            
            if (galleryItem == null)
            {
                return NotFound(new { error = "Gallery item not found." });
            }

            return Ok(galleryItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
