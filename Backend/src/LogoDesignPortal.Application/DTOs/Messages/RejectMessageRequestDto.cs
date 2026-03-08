namespace LogoDesignPortal.Application.DTOs.Messages;

public class RejectMessageRequestDto
{
    public Guid MessageId { get; set; }
    public string? Reason { get; set; }
}
