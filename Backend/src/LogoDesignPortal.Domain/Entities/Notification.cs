using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; } // User to notify
    public Guid? OrderId { get; set; } // Related order (optional)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public LogoOrder? Order { get; set; }
}
