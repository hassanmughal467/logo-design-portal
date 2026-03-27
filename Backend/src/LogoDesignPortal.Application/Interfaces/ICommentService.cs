using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponseDto> CreateCommentAsync(Guid orderId, CreateCommentRequestDto request, Guid createdBy, string? userRole);
    Task<List<CommentResponseDto>> GetOrderCommentsAsync(Guid orderId, Guid? userId, string? userRole);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, string userRole);
    Task<CommentResponseDto?> SetCommentVisibilityAsync(Guid commentId, SetCommentVisibilityRequestDto request, Guid userId, string userRole);
    Task MarkOrderCommentsAsReadAsync(Guid orderId, Guid userId, string userRole);
    Task<OrderCommentUnreadCountsDto> GetOrderCommentUnreadCountsAsync(Guid orderId, Guid userId, string? userRole);

    /// <summary>Appends a price-negotiation thread entry (workflow or system). Does not enforce the locked-order rule used for manual comments.</summary>
    Task AppendPriceNegotiationNoteAsync(Guid orderId, Guid createdBy, CommentType channel, string content);
}

