using AutoMapper;
using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CommentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CommentResponseDto> CreateCommentAsync(Guid orderId, CreateCommentRequestDto request, Guid createdBy)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
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
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
        {
            return false;
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
