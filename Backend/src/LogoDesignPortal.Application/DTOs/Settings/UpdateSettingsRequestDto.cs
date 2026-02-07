namespace LogoDesignPortal.Application.DTOs.Settings;

public class UpdateSettingsRequestDto
{
    public string Category { get; set; } = string.Empty; // Business, Brand, InvoiceTemplate, PaymentMethods, Notifications
    public Dictionary<string, object> Data { get; set; } = new();
}
