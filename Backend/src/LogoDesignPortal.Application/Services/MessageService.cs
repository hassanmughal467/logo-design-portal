using AutoMapper;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class MessageService : IMessageService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public MessageService(IApplicationDbContext context, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
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

        // Reload with includes for MapToDto
        message = await _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .FirstAsync(m => m.Id == message.Id);

        // Notify recipient(s): New message
        try
        {
            var senderName = $"{sender.FirstName} {sender.LastName}".Trim();
            if (string.IsNullOrEmpty(senderName)) senderName = "Someone";
            var orderNumber = request.OrderId.HasValue ? NotificationFormatHelper.GetOrderNumber(request.OrderId.Value) : "N/A";
            var title = "New Message";
            var senderRole = sender.Role?.Name ?? string.Empty;

            if (senderRole == "Client")
            {
                // Client sends: notify Admin, SuperAdmin, and assigned Designer
                var messageText = $"{senderName} sent a new message regarding order (#{orderNumber})";
                var refId = request.OrderId ?? message.Id;
                var refType = request.OrderId.HasValue ? NotificationReferenceType.Order : NotificationReferenceType.Message;
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, messageText, NotificationType.Info, refType, refId, senderId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, messageText, NotificationType.Info, refType, refId, senderId);
                if (request.OrderId.HasValue)
                {
                    var designerUserId = await _context.LogoOrders
                        .Where(o => o.Id == request.OrderId.Value && !o.IsDeleted && o.DesignerId.HasValue)
                        .Select(o => o.DesignerId)
                        .FirstOrDefaultAsync();
                    if (designerUserId.HasValue)
                    {
                        var designerUser = await _context.DesignerProfiles
                            .Where(d => d.Id == designerUserId.Value && !d.IsDeleted)
                            .Select(d => d.UserId)
                            .FirstOrDefaultAsync();
                        if (designerUser != Guid.Empty)
                        {
                            await _notificationService.CreateNotificationAsync(
                                designerUser,
                                title,
                                messageText,
                                NotificationType.Info,
                                request.OrderId,
                                NotificationReferenceType.Message,
                                message.Id,
                                senderId
                            );
                        }
                    }
                }
            }
            else if (request.RecipientId.HasValue && request.RecipientId != senderId)
            {
                // Admin/Designer sends: notify client (recipient)
                var messageText = $"{senderName} sent a new message regarding order (#{orderNumber})";
                await _notificationService.CreateNotificationAsync(
                    request.RecipientId.Value,
                    title,
                    messageText,
                    NotificationType.Info,
                    request.OrderId,
                    NotificationReferenceType.Message,
                    message.Id,
                    senderId
                );
            }
        }
        catch
        {
            // Must not fail message creation
        }

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
