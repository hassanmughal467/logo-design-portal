using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Gallery;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;

    public GalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    /// <summary>Lists approved gallery assets for the authenticated client.</summary>
    [HttpGet("my-gallery")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(List<GalleryItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyGallery(CancellationToken cancellationToken)
    {
        var userId = User.GetUserIdOrThrow();
        var galleryItems = await _galleryService.GetClientGalleryAsync(userId, cancellationToken);
        return Ok(galleryItems);
    }

    /// <summary>Gets a single gallery item if it belongs to the authenticated client.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(GalleryItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGalleryItemById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserIdOrThrow();
        var galleryItem = await _galleryService.GetGalleryItemByIdAsync(id, userId, cancellationToken);

        if (galleryItem == null)
        {
            return NotFound(new { error = "Gallery item not found." });
        }

        return Ok(galleryItem);
    }
}
