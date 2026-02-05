using LogoDesignPortal.Application.DTOs.Users;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientInfoDto? Client { get; set; }
    public DesignerInfoDto? Designer { get; set; }
    public int FileCount { get; set; }
}
