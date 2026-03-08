namespace LogoDesignPortal.Application.DTOs.Messages;

public class ForwardMessageRequestDto
{
    public Guid MessageId { get; set; }
    public Guid TargetUserId { get; set; }
    public string? EditedContent { get; set; }
}
