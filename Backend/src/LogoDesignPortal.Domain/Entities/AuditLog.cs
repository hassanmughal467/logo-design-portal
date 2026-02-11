namespace LogoDesignPortal.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Order, Invoice, User, File, Project
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // created, updated, status_changed, assigned, cancelled, paid, uploaded, deleted_soft
    public string? PreviousValue { get; set; } // JSON, nullable
    public string? NewValue { get; set; } // JSON, nullable
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByRole { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
