namespace LogoDesignPortal.Application.DTOs.ClientLogoPricing;

public class ClientLogoPricingResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public int DesignCategory { get; set; }
    public string DesignCategoryName { get; set; } = string.Empty;
    public int DesignType { get; set; }
    public string DesignTypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
