namespace LogoDesignPortal.Application.DTOs.Messages;

public class CreateMessageRequestDto
{
    public Guid? OrderId { get; set; }
    public Guid? RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
}
