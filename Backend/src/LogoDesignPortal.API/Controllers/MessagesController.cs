using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequestDto request)
    {
        try
        {
            var senderId = User.GetUserIdOrThrow();
            var message = await _messageService.CreateMessageAsync(request, senderId);
            return CreatedAtAction(nameof(GetMessageById), new { id = message.Id }, message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessageById(Guid id)
    {
        var userId = User.GetUserIdOrThrow();
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var message = await _messageService.GetMessageByIdAsync(id, userId, userRole);
        if (message == null)
        {
            return NotFound(new { error = "Message not found." });
        }
        return Ok(message);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MessageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages()
    {
        var userId = User.GetUserIdOrThrow();
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var messages = await _messageService.GetMessagesAsync(userId, userRole);
        return Ok(messages);
    }

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(List<MessageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesByOrder(Guid orderId)
    {
        var userId = User.GetUserIdOrThrow();
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var messages = await _messageService.GetMessagesByOrderAsync(orderId, userId, userRole);
        return Ok(messages);
    }

    [HttpPut("{id}/read")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = User.GetUserIdOrThrow();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var message = await _messageService.MarkAsReadAsync(id, userId, userRole);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("forward")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForwardMessage([FromBody] ForwardMessageRequestDto request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            var message = await _messageService.ForwardMessageAsync(request, adminUserId);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectMessage([FromBody] RejectMessageRequestDto request)
    {
        try
        {
            var adminUserId = User.GetUserIdOrThrow();
            var message = await _messageService.RejectMessageAsync(request, adminUserId);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
