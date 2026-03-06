using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; } // User to notify
    public Guid? OrderId { get; set; } // Related order (optional, backward compatibility)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public NotificationReferenceType ReferenceType { get; set; } = NotificationReferenceType.Order;
    public Guid? ReferenceId { get; set; } // OrderId, InvoiceId, MessageId, etc.
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Number of similar events aggregated into this notification (1 = single event).
    /// </summary>
    public int AggregationCount { get; set; } = 1;

    /// <summary>
    /// Timestamp of the most recent occurrence when aggregated.
    /// </summary>
    public DateTime? LastOccurrenceAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public LogoOrder? Order { get; set; }
}
