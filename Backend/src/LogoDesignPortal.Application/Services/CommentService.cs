using AutoMapper;
using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public CommentService(IApplicationDbContext context, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<CommentResponseDto> CreateCommentAsync(Guid orderId, CreateCommentRequestDto request, Guid createdBy)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == createdBy && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var comment = new OrderComment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Content = request.Content,
            CreatedBy = createdBy,
            IsInternal = request.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderComments.Add(comment);
        await _context.SaveChangesAsync();

        // Notify Admin and SuperAdmin when Designer or Client adds a comment
        var creatorRole = user.Role?.Name ?? string.Empty;
        if (creatorRole == "Designer" || creatorRole == "Client")
        {
            try
            {
                var creatorName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrEmpty(creatorName)) creatorName = creatorRole == "Designer" ? "Designer" : "Client";
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = creatorRole == "Designer" ? "New Comment from Designer" : "New Comment from Client";
                var message = $"{creatorName} added a comment on order (#{orderNumber})";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, createdBy);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, createdBy);
            }
            catch
            {
                // Notification failure must not affect comment creation
            }
        }

        var response = _mapper.Map<CommentResponseDto>(comment);
        response.CreatedByName = $"{user.FirstName} {user.LastName}";
        response.CreatedByRole = user.Role?.Name ?? string.Empty;

        return response;
    }

    public async Task<List<CommentResponseDto>> GetOrderCommentsAsync(Guid orderId, Guid? userId, string? userRole)
    {
        var query = _context.OrderComments
            .Include(c => c.CreatedByUser)
                .ThenInclude(u => u.Role)
            .Where(c => c.OrderId == orderId && !c.IsDeleted);

        // Clients can only see non-internal comments
        if (userRole == "Client")
        {
            query = query.Where(c => !c.IsInternal);
        }

        var comments = await query.OrderBy(c => c.CreatedAt).ToListAsync();
        return _mapper.Map<List<CommentResponseDto>>(comments);
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, string userRole)
    {
        var comment = await _context.OrderComments
            .Include(c => c.Order)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
        {
            return false;
        }

        if (comment.Order != null && OrderLockingHelper.IsOrderLocked(comment.Order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        // Only allow deletion by creator or admin/superadmin
        if (comment.CreatedBy != userId && userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this comment.");
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedBy = userId;
        await _context.SaveChangesAsync();

        return true;
    }
}
