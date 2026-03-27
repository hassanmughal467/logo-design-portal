using AutoMapper;
using LogoDesignPortal.Application.DTOs.Messages;
using LogoDesignPortal.Application.Exceptions;
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
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new InvalidOperationException("Message content cannot be empty.");
        }

        var sender = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == senderId);

        if (sender == null)
        {
            throw new InvalidOperationException("Sender not found.");
        }

        var senderRole = sender.Role?.Name ?? string.Empty;

        // Security: Client and Designer cannot message each other directly - must go through Admin
        if (request.RecipientId.HasValue)
        {
            var recipient = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == request.RecipientId.Value);
            if (recipient == null)
            {
                throw new InvalidOperationException("Recipient not found.");
            }
            var recipientRole = recipient?.Role?.Name ?? string.Empty;

            if (senderRole == "Client" && recipientRole == "Designer")
            {
                throw new InvalidOperationException("Clients cannot message designers directly. Your message will be sent to the admin team for review.");
            }
            if (senderRole == "Designer" && recipientRole == "Client")
            {
                throw new InvalidOperationException("Designers cannot message clients directly. Your message will be sent to the admin team for review.");
            }
        }

        // Block all roles from sending order-related messages for locked (terminal) orders
        // Verify sender has access to the order when OrderId is provided
        if (request.OrderId.HasValue)
        {
            var order = await _context.LogoOrders
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId.Value && !o.IsDeleted);
            if (order != null)
            {
                if (OrderLockingHelper.IsOrderLocked(order.Status))
                {
                    throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
                }
                // Verify order access: Client must own order; Designer must be assigned
                if (senderRole == "Client")
                {
                    if (order.Client == null || order.Client.UserId != senderId)
                        throw new ForbiddenAccessException("You don't have access to send messages for this order.");
                }
                else if (senderRole == "Designer")
                {
                    var designer = await _context.DesignerProfiles.FirstOrDefaultAsync(d => d.UserId == senderId && !d.IsDeleted);
                    if (designer == null || order.DesignerId != designer.Id)
                        throw new ForbiddenAccessException("You don't have access to send messages for this order.");
                }
                // Admin/SuperAdmin: allow
            }
        }

        var requiresAdminApproval = senderRole == "Client" || senderRole == "Designer";
        var message = new Message
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            SenderId = senderId,
            RecipientId = requiresAdminApproval ? null : request.RecipientId,
            Content = request.Content,
            CreatedBy = senderId,
            RequiresAdminApproval = requiresAdminApproval,
            OriginalSenderRole = senderRole == "Client" ? MessageSenderRole.Client : senderRole == "Designer" ? MessageSenderRole.Designer : null
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
            var refId = request.OrderId ?? message.Id;
            var refType = request.OrderId.HasValue ? NotificationReferenceType.Order : NotificationReferenceType.Message;

            if (senderRole == "Client")
            {
                // Client sends: notify Admin and SuperAdmin only (mediated workflow)
                var messageText = $"{senderName} sent a new message regarding order (#{orderNumber})";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, messageText, NotificationType.Info, refType, refId, senderId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, messageText, NotificationType.Info, refType, refId, senderId);
            }
            else if (senderRole == "Designer")
            {
                // Designer sends: notify Admin and SuperAdmin only (mediated workflow)
                var messageText = $"{senderName} sent a new message regarding order (#{orderNumber})";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, messageText, NotificationType.Info, refType, refId, senderId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, messageText, NotificationType.Info, refType, refId, senderId);
            }
            else if (request.RecipientId.HasValue && request.RecipientId != senderId)
            {
                // Admin/SuperAdmin sends: notify recipient directly
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

        return MapToDto(message, null);
    }

    public async Task<MessageResponseDto?> GetMessageByIdAsync(Guid messageId, Guid userId, string? userRole)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

        if (message == null)
        {
            return null;
        }

        var isAdmin = userRole == "Admin" || userRole == "SuperAdmin";
        var hasAccess = message.SenderId == userId || message.RecipientId == userId ||
            (isAdmin && message.RequiresAdminApproval && !message.IsRejected && message.ForwardedToMessageId == null);

        if (!hasAccess)
        {
            return null;
        }

        return MapToDto(message, userRole);
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

        if (userId.HasValue)
        {
            var isAdmin = userRole == "Admin" || userRole == "SuperAdmin";
            if (isAdmin)
            {
                query = query.Where(m =>
                    m.SenderId == userId.Value ||
                    m.RecipientId == userId.Value ||
                    (m.RequiresAdminApproval && !m.IsRejected && m.ForwardedToMessageId == null));
            }
            else
            {
                query = query.Where(m => m.SenderId == userId.Value || m.RecipientId == userId.Value);
            }
        }

        var messages = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return messages.Select(m => MapToDto(m, userRole)).ToList();
    }

    public async Task<List<MessageResponseDto>> GetMessagesByOrderAsync(Guid orderId, Guid userId, string? userRole)
    {
        var isAdmin = userRole == "Admin" || userRole == "SuperAdmin";
        var baseFilter = _context.Messages
            .Include(m => m.Sender)
                .ThenInclude(u => u.Role)
            .Include(m => m.Recipient)
                .ThenInclude(u => u.Role)
            .Where(m => m.OrderId == orderId && !m.IsDeleted);

        var messages = isAdmin
            ? await baseFilter
                .Where(m => m.SenderId == userId || m.RecipientId == userId ||
                    (m.RequiresAdminApproval && !m.IsRejected && m.ForwardedToMessageId == null))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync()
            : await baseFilter
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

        return messages.Select(m => MapToDto(m, userRole)).ToList();
    }

    public async Task<MessageResponseDto> MarkAsReadAsync(Guid messageId, Guid userId, string? userRole = null)
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

        var isAdmin = userRole == "Admin" || userRole == "SuperAdmin";
        if (message.RecipientId == userId || (message.RequiresAdminApproval && isAdmin))
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(message, userRole);
    }

    public async Task<MessageResponseDto> ForwardMessageAsync(ForwardMessageRequestDto request, Guid adminUserId)
    {
        var original = await _context.Messages
            .Include(m => m.Order)
            .Include(m => m.Sender).ThenInclude(u => u.Role)
            .Include(m => m.Recipient).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId && !m.IsDeleted);

        if (original == null || !original.RequiresAdminApproval || original.IsRejected)
        {
            throw new InvalidOperationException("Message not found or cannot be forwarded.");
        }

        if (original.Order != null && OrderLockingHelper.IsOrderLocked(original.Order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        var content = string.IsNullOrWhiteSpace(request.EditedContent) ? original.Content : request.EditedContent.Trim();
        var forwarded = new Message
        {
            Id = Guid.NewGuid(),
            OrderId = original.OrderId,
            SenderId = adminUserId,
            RecipientId = request.TargetUserId,
            Content = content,
            ForwardedByAdmin = true,
            OriginalSenderRole = original.OriginalSenderRole,
            CreatedBy = adminUserId
        };
        _context.Messages.Add(forwarded);

        original.ForwardedToMessageId = forwarded.Id;
        original.UpdatedAt = DateTime.UtcNow;
        original.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();

        forwarded = await _context.Messages
            .Include(m => m.Sender).ThenInclude(u => u.Role)
            .Include(m => m.Recipient).ThenInclude(u => u.Role)
            .FirstAsync(m => m.Id == forwarded.Id);

        var orderNumber = original.OrderId.HasValue ? NotificationFormatHelper.GetOrderNumber(original.OrderId.Value) : "N/A";
        try
        {
            await _notificationService.CreateNotificationAsync(
                request.TargetUserId,
                "New Message",
                $"You have a new message regarding order (#{orderNumber})",
                NotificationType.Info,
                original.OrderId,
                NotificationReferenceType.Message,
                forwarded.Id,
                adminUserId
            );
        }
        catch { /* non-fatal */ }

        return MapToDto(forwarded, "Admin");
    }

    public async Task<MessageResponseDto> RejectMessageAsync(RejectMessageRequestDto request, Guid adminUserId)
    {
        var message = await _context.Messages
            .Include(m => m.Order)
            .Include(m => m.Sender).ThenInclude(u => u.Role)
            .Include(m => m.Recipient).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId && !m.IsDeleted);

        if (message == null || !message.RequiresAdminApproval || message.IsRejected)
        {
            throw new InvalidOperationException("Message not found or cannot be rejected.");
        }

        if (message.Order != null && OrderLockingHelper.IsOrderLocked(message.Order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        message.IsRejected = true;
        message.RejectedAt = DateTime.UtcNow;
        message.RejectedBy = adminUserId;
        message.UpdatedAt = DateTime.UtcNow;
        message.UpdatedBy = adminUserId;
        await _context.SaveChangesAsync();

        return MapToDto(message, "Admin");
    }

    private MessageResponseDto MapToDto(Message message, string? userRole)
    {
        var senderRole = message.Sender?.Role?.Name;
        var recipientRole = message.Recipient?.Role?.Name;

        string senderName = $"{message.Sender?.FirstName} {message.Sender?.LastName}".Trim();
        if (string.IsNullOrEmpty(senderName)) senderName = "Someone";
        string? recipientName = message.Recipient != null ? $"{message.Recipient.FirstName} {message.Recipient.LastName}" : null;

        // Identity masking: Client must not see designer name; Designer must not see client name
        if (userRole == "Client" && senderRole == "Designer") senderName = "Company Design Team";
        if (userRole == "Client" && recipientRole == "Designer") recipientName = "Company Design Team";
        if (userRole == "Designer" && senderRole == "Client") senderName = "Company Project";
        if (userRole == "Designer" && recipientRole == "Client") recipientName = "Company Project";
        if (message.ForwardedByAdmin && message.OriginalSenderRole.HasValue)
        {
            if (userRole == "Client" && message.OriginalSenderRole == MessageSenderRole.Designer) senderName = "Company Design Team";
            if (userRole == "Designer" && message.OriginalSenderRole == MessageSenderRole.Client) senderName = "Company Project";
        }

        return new MessageResponseDto
        {
            Id = message.Id,
            OrderId = message.OrderId,
            SenderId = message.SenderId,
            SenderName = senderName,
            SenderRole = senderRole,
            RecipientId = message.RecipientId,
            RecipientName = recipientName ?? (message.RequiresAdminApproval ? "Admin" : null),
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt,
            RequiresAdminApproval = message.RequiresAdminApproval,
            ForwardedByAdmin = message.ForwardedByAdmin,
            OriginalSenderRole = message.OriginalSenderRole?.ToString(),
            IsRejected = message.IsRejected
        };
    }
}
