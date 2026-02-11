namespace LogoDesignPortal.Application.DTOs.Settings;

public class SettingsResponseDto
{
    public Dictionary<string, string> Business { get; set; } = new();
    public Dictionary<string, string> Brand { get; set; } = new();
    public Dictionary<string, string> InvoiceTemplate { get; set; } = new();
    public Dictionary<string, string> Invoice { get; set; } = new(); // Invoice prefix, due days, tax percentage, currency
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
    public Dictionary<string, bool> Notifications { get; set; } = new();
}

public class PaymentMethodDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AccountDetails { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
