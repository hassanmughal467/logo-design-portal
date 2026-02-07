namespace LogoDesignPortal.Domain.Entities;

public class Settings : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Business, Brand, Invoice, Payment, Notification
    public string? Description { get; set; }
}
