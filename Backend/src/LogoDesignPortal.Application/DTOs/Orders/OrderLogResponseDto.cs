namespace LogoDesignPortal.Application.DTOs.Orders;

public class OrderLogResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public Guid? PerformedById { get; set; }
    public string? Note { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
