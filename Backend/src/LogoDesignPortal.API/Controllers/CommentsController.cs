using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        ICommentService commentService,
        ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpPost("orders/{orderId}")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateComment(Guid orderId, [FromBody] CreateCommentRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var comment = await _commentService.CreateCommentAsync(orderId, request, userId, userRole);
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
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

    [HttpGet("orders/{orderId}")]
    [ProducesResponseType(typeof(List<CommentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderComments(Guid orderId)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var comments = await _commentService.GetOrderCommentsAsync(orderId, userId, userRole);
            return Ok(comments);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpPost("orders/{orderId}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkOrderCommentsAsRead(Guid orderId)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;
            await _commentService.MarkOrderCommentsAsReadAsync(orderId, userId, userRole);
            return Ok(new { message = "Comments marked as read." });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("orders/{orderId}/unread-counts")]
    [ProducesResponseType(typeof(OrderCommentUnreadCountsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderCommentUnreadCounts(Guid orderId)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var counts = await _commentService.GetOrderCommentUnreadCountsAsync(orderId, userId, userRole);
            return Ok(counts);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpPut("{id}/visibility")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCommentVisibility(Guid id, [FromBody] SetCommentVisibilityRequestDto request)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;
            var comment = await _commentService.SetCommentVisibilityAsync(id, request, userId, userRole);
            if (comment == null)
                return NotFound(new { error = "Comment not found." });
            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(Guid id)
    {
        // This would need to be implemented in the service
        return NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;
            var result = await _commentService.DeleteCommentAsync(id, userId, userRole);
            
            if (!result)
            {
                return NotFound(new { error = "Comment not found." });
            }

            return Ok(new { message = "Comment deleted successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
