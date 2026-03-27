using AutoMapper;
using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Application.Exceptions;
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

    /// <summary>
    /// Validates that the user has access to the order.
    /// Client → own orders only; Designer → assigned orders only; Admin/SuperAdmin → all.
    /// </summary>
    private async Task<LogoOrder> EnsureOrderAccessAsync(Guid orderId, Guid userId, string? userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (userRole == "Client")
        {
            if (order.Client?.UserId != userId)
                throw new ForbiddenAccessException("You don't have access to this order.");
        }
        else if (userRole == "Designer")
        {
            if (order.DesignerId == null)
                throw new ForbiddenAccessException("You don't have access to this order.");
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to this order.");
        }
        else if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new ForbiddenAccessException("You don't have access to this order.");
        }

        return order;
    }

    public async Task<CommentResponseDto> CreateCommentAsync(Guid orderId, CreateCommentRequestDto request, Guid createdBy, string? userRole)
    {
        var order = await EnsureOrderAccessAsync(orderId, createdBy, userRole);

        if (OrderLockingHelper.IsOrderLocked(order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == createdBy && !u.IsDeleted);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        var creatorRole = user.Role?.Name ?? string.Empty;

        // Clients cannot create internal comments
        var isInternal = creatorRole == "Client" ? false : request.IsInternal;

        // Designer comments default to not visible to client until Admin approves
        var visibleToClient = creatorRole == "Designer" ? false : !isInternal;
        var commentType = creatorRole == "Designer" ? CommentType.DesignerFeedback
            : isInternal ? CommentType.Internal
            : CommentType.General;

        var commentLegacy = new OrderComment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Content = request.Content,
            CreatedBy = createdBy,
            IsInternal = isInternal,
            CommentType = commentType,
            VisibleToClient = visibleToClient,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderComments.Add(commentLegacy);
        await _context.SaveChangesAsync();

        // Notify Admin and SuperAdmin when Designer or Client adds a comment
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

        return MapToResponse(commentLegacy, user, userRole);
    }

    public async Task<List<CommentResponseDto>> GetOrderCommentsAsync(Guid orderId, Guid? userId, string? userRole)
    {
        await EnsureOrderAccessAsync(orderId, userId ?? Guid.Empty, userRole);

        var query = _context.OrderComments
            .Include(c => c.CreatedByUser)
                .ThenInclude(u => u.Role)
            .Where(c => c.OrderId == orderId && !c.IsDeleted);

        query = ApplyOrderCommentVisibilityFilter(query, userRole);

        var comments = await query.OrderBy(c => c.CreatedAt).ToListAsync();
        var dtos = new List<CommentResponseDto>();

        foreach (var c in comments)
        {
            var dto = _mapper.Map<CommentResponseDto>(c);
            var creatorRole = c.CreatedByUser.Role?.Name ?? string.Empty;
            dto.CreatedByName = $"{c.CreatedByUser.FirstName} {c.CreatedByUser.LastName}".Trim();
            if (string.IsNullOrEmpty(dto.CreatedByName)) dto.CreatedByName = creatorRole;
            dto.CreatedByRole = creatorRole;
            dto.CommentType = c.CommentType.ToString();

            // Mask designer identity when viewer is Client
            if (userRole == "Client" && creatorRole == "Designer")
                dto.CreatedByName = "Design Team";

            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, string userRole)
    {
        var comment = await _context.OrderComments
            .Include(c => c.Order)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return false;

        await EnsureOrderAccessAsync(comment.OrderId, userId, userRole);

        if (comment.Order != null && OrderLockingHelper.IsOrderLocked(comment.Order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        if (comment.CreatedBy != userId && userRole != "Admin" && userRole != "SuperAdmin")
            throw new UnauthorizedAccessException("You don't have permission to delete this comment.");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedBy = userId;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CommentResponseDto?> SetCommentVisibilityAsync(Guid commentId, SetCommentVisibilityRequestDto request, Guid userId, string userRole)
    {
        if (userRole != "Admin" && userRole != "SuperAdmin")
            throw new UnauthorizedAccessException("Only Admin can approve comment visibility.");

        var comment = await _context.OrderComments
            .Include(c => c.CreatedByUser)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return null;

        await EnsureOrderAccessAsync(comment.OrderId, userId, userRole);

        if (comment.CommentType == CommentType.PriceNegotiationClient ||
            comment.CommentType == CommentType.PriceNegotiationDesigner)
            throw new InvalidOperationException("Price negotiation notes use fixed visibility and cannot be changed here.");

        comment.VisibleToClient = request.VisibleToClient;
        await _context.SaveChangesAsync();

        return MapToResponse(comment, comment.CreatedByUser, userRole);
    }

    public async Task MarkOrderCommentsAsReadAsync(Guid orderId, Guid userId, string userRole)
    {
        await EnsureOrderAccessAsync(orderId, userId, userRole);

        var comments = await ApplyOrderCommentVisibilityFilter(
                _context.OrderComments
                    .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Role)
                    .Where(c => c.OrderId == orderId && !c.IsDeleted),
                userRole)
            .ToListAsync();

        foreach (var c in comments)
        {
            if (userRole == "Client" && !c.IsReadByClient)
                c.IsReadByClient = true;
            else if (userRole == "Designer" && !c.IsReadByDesigner)
                c.IsReadByDesigner = true;
            else if ((userRole == "Admin" || userRole == "SuperAdmin") && !c.IsReadByAdmin)
                c.IsReadByAdmin = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<OrderCommentUnreadCountsDto> GetOrderCommentUnreadCountsAsync(Guid orderId, Guid userId, string? userRole)
    {
        await EnsureOrderAccessAsync(orderId, userId, userRole);

        var visible = ApplyOrderCommentVisibilityFilter(
            _context.OrderComments
                .Include(c => c.CreatedByUser)
                .ThenInclude(u => u.Role)
                .Where(c => c.OrderId == orderId && !c.IsDeleted),
            userRole);

        var generalComments = visible.Where(c =>
            c.CommentType != CommentType.PriceNegotiationClient && c.CommentType != CommentType.PriceNegotiationDesigner);
        var priceThreads = visible.Where(c =>
            c.CommentType == CommentType.PriceNegotiationClient || c.CommentType == CommentType.PriceNegotiationDesigner);

        var unreadGeneral = userRole == "Client"
            ? await generalComments.CountAsync(c => !c.IsReadByClient)
            : userRole == "Designer"
                ? await generalComments.CountAsync(c => !c.IsReadByDesigner)
                : await generalComments.CountAsync(c => !c.IsReadByAdmin);
        var unreadPrice = userRole == "Client"
            ? await priceThreads.CountAsync(c => !c.IsReadByClient)
            : userRole == "Designer"
                ? await priceThreads.CountAsync(c => !c.IsReadByDesigner)
                : await priceThreads.CountAsync(c => !c.IsReadByAdmin);

        return new OrderCommentUnreadCountsDto
        {
            UnreadFiles = 0,
            UnreadRevisions = 0,
            UnreadComments = unreadGeneral,
            UnreadPriceNegotiationNotes = unreadPrice
        };
    }

    public async Task AppendPriceNegotiationNoteAsync(Guid orderId, Guid createdBy, CommentType channel, string content)
    {
        if (channel != CommentType.PriceNegotiationClient && channel != CommentType.PriceNegotiationDesigner)
            throw new ArgumentException("Channel must be a price negotiation comment type.", nameof(channel));

        if (string.IsNullOrWhiteSpace(content))
            return;

        var trimmed = content.Trim();
        if (trimmed.Length > 2000)
            trimmed = trimmed[..2000];

        var order = await _context.LogoOrders.FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == createdBy && !u.IsDeleted);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var comment = new OrderComment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Content = trimmed,
            CreatedBy = createdBy,
            IsInternal = channel == CommentType.PriceNegotiationDesigner,
            CommentType = channel,
            VisibleToClient = channel == CommentType.PriceNegotiationClient,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderComments.Add(comment);
        await _context.SaveChangesAsync();
    }

    private static IQueryable<OrderComment> ApplyOrderCommentVisibilityFilter(IQueryable<OrderComment> query, string? userRole)
    {
        // Designers do not see client-authored thread comments; Admin/SuperAdmin mediate and relay.
        if (userRole == "Designer")
        {
            query = query.Where(c => c.CommentType != CommentType.PriceNegotiationClient);
            query = query.Where(c => c.CreatedByUser.Role == null || c.CreatedByUser.Role.Name != "Client");
        }

        if (userRole == "Client")
        {
            query = query.Where(c => c.CommentType != CommentType.PriceNegotiationDesigner);
            query = query.Where(c => !c.IsInternal && (c.VisibleToClient
                || (c.CreatedByUser.Role != null && (c.CreatedByUser.Role.Name == "Client" || c.CreatedByUser.Role.Name == "Admin" || c.CreatedByUser.Role.Name == "SuperAdmin"))));
        }

        return query;
    }

    private static CommentResponseDto MapToResponse(OrderComment comment, User createdByUser, string? viewerRole)
    {
        var creatorRole = createdByUser.Role?.Name ?? string.Empty;
        var createdByName = $"{createdByUser.FirstName} {createdByUser.LastName}".Trim();
        if (string.IsNullOrEmpty(createdByName)) createdByName = creatorRole;

        if (viewerRole == "Client" && creatorRole == "Designer")
            createdByName = "Design Team";

        return new CommentResponseDto
        {
            Id = comment.Id,
            OrderId = comment.OrderId,
            Content = comment.Content,
            CreatedBy = comment.CreatedBy,
            CreatedByName = createdByName,
            CreatedByRole = creatorRole,
            IsInternal = comment.IsInternal,
            CommentType = comment.CommentType.ToString(),
            VisibleToClient = comment.VisibleToClient,
            IsReadByClient = comment.IsReadByClient,
            IsReadByDesigner = comment.IsReadByDesigner,
            IsReadByAdmin = comment.IsReadByAdmin,
            CreatedAt = comment.CreatedAt
        };
    }
}
