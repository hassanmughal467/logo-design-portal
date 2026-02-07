namespace LogoDesignPortal.Application.DTOs.Reviews;

public class CreateReviewRequestDto
{
    public Guid OrderId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
