using LogoDesignPortal.Application.DTOs.Messages;

namespace LogoDesignPortal.Application.Interfaces;

public interface IMessageService
{
    Task<MessageResponseDto> CreateMessageAsync(CreateMessageRequestDto request, Guid senderId);
    Task<List<MessageResponseDto>> GetMessagesAsync(Guid? userId, string? userRole);
    Task<List<MessageResponseDto>> GetMessagesByOrderAsync(Guid orderId, Guid userId);
    Task<MessageResponseDto> MarkAsReadAsync(Guid messageId, Guid userId);
}
