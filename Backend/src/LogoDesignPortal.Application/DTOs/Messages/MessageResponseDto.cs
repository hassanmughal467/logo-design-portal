namespace LogoDesignPortal.Application.DTOs.Messages;

public class MessageResponseDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderRole { get; set; }
    public Guid? RecipientId { get; set; }
    public string? RecipientName { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool ForwardedByAdmin { get; set; }
    public string? OriginalSenderRole { get; set; }
    public bool IsRejected { get; set; }
}
