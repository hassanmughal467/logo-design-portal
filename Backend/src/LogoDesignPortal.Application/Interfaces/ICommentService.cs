using LogoDesignPortal.Application.DTOs.Comments;

namespace LogoDesignPortal.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponseDto> CreateCommentAsync(Guid orderId, CreateCommentRequestDto request, Guid createdBy);
    Task<List<CommentResponseDto>> GetOrderCommentsAsync(Guid orderId, Guid? userId, string? userRole);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, string userRole);
}
