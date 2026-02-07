using AutoMapper;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class MessageService : IMessageService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MessageService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MessageResponseDto> CreateMessageAsync(CreateMessageRequestDto request, Guid senderId)
    {
        var sender = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == senderId);

        if (sender == null)
        {
            throw new InvalidOperationException("Sender not found.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            SenderId = senderId,
            RecipientId = request.RecipientId,
            Content = request.Content,
            CreatedBy = senderId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return MapToDto(message);
    }

    public async Task<List<MessageResponseDto>> GetMessagesAsync(Guid? userId, string? userRole)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        // Filter by user - show messages where user is sender or recipient
        if (userId.HasValue)
        {
            query = query.Where(m => m.SenderId == userId.Value || m.RecipientId == userId.Value);
        }

        var messages = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return messages.Select(m => MapToDto(m)).ToList();
    }

    public async Task<List<MessageResponseDto>> GetMessagesByOrderAsync(Guid orderId, Guid userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .Where(m => m.OrderId == orderId && !m.IsDeleted && (m.SenderId == userId || m.RecipientId == userId))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => MapToDto(m)).ToList();
    }

    public async Task<MessageResponseDto> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

        if (message == null)
        {
            throw new InvalidOperationException("Message not found.");
        }

        if (message.RecipientId == userId)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(message);
    }

    private MessageResponseDto MapToDto(Message message)
    {
        return new MessageResponseDto
        {
            Id = message.Id,
            OrderId = message.OrderId,
            SenderId = message.SenderId,
            SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
            SenderRole = message.Sender.Role?.Name,
            RecipientId = message.RecipientId,
            RecipientName = message.Recipient != null ? $"{message.Recipient.FirstName} {message.Recipient.LastName}" : null,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt
        };
    }
}
