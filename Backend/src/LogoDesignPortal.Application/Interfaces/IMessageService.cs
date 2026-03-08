using LogoDesignPortal.Application.DTOs.Messages;

namespace LogoDesignPortal.Application.Interfaces;

public interface IMessageService
{
    Task<MessageResponseDto> CreateMessageAsync(CreateMessageRequestDto request, Guid senderId);
    Task<MessageResponseDto?> GetMessageByIdAsync(Guid messageId, Guid userId, string? userRole);
    Task<List<MessageResponseDto>> GetMessagesAsync(Guid? userId, string? userRole);
    Task<List<MessageResponseDto>> GetMessagesByOrderAsync(Guid orderId, Guid userId, string? userRole = null);
    Task<MessageResponseDto> MarkAsReadAsync(Guid messageId, Guid userId, string? userRole = null);
    Task<MessageResponseDto> ForwardMessageAsync(ForwardMessageRequestDto request, Guid adminUserId);
    Task<MessageResponseDto> RejectMessageAsync(RejectMessageRequestDto request, Guid adminUserId);
}
