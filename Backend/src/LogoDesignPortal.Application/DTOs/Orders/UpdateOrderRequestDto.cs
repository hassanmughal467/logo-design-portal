using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class UpdateOrderRequestDto
{
    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public OrderPriority? Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Instructions { get; set; }
    public string? RequiredFormats { get; set; }
    public string? Requirements { get; set; }
    public string? ColorPreferences { get; set; }
    public string? StylePreferences { get; set; }
}
