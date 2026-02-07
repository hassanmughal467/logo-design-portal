namespace LogoDesignPortal.Application.DTOs.Reviews;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
