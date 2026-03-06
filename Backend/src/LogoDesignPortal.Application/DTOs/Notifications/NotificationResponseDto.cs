namespace LogoDesignPortal.Application.DTOs.Notifications;

public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = "Order";
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Number of similar events aggregated (1 = single event).
    /// </summary>
    public int AggregationCount { get; set; } = 1;

    /// <summary>
    /// Timestamp of the most recent occurrence when aggregated.
    /// </summary>
    public DateTime? LastOccurrenceAt { get; set; }
}
