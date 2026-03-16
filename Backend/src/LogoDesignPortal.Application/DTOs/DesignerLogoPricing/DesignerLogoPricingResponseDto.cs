namespace LogoDesignPortal.Application.DTOs.DesignerLogoPricing;

public class DesignerLogoPricingResponseDto
{
    public Guid Id { get; set; }
    public Guid DesignerId { get; set; }
    public string? DesignerName { get; set; }
    public int DesignCategory { get; set; }
    public string DesignCategoryName { get; set; } = string.Empty;
    public int DesignType { get; set; }
    public string DesignTypeName { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
