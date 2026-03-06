namespace LogoDesignPortal.Application.DTOs.Notifications;

public class NotificationListResponseDto
{
    public List<NotificationResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}
