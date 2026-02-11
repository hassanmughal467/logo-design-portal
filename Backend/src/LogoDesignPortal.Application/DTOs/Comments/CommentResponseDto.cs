namespace LogoDesignPortal.Application.DTOs.Comments;

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
