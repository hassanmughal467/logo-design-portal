namespace LogoDesignPortal.Application.DTOs.AuditLogs;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByName { get; set; }
    public string? PerformedByRole { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
}
