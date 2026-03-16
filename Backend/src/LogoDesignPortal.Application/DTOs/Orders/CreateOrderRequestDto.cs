using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class CreateOrderRequestDto
{
    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional. Admin sets price when no client pricing exists. Max 100,000.</summary>
    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
    public decimal Price { get; set; }

    public OrderPriority? Priority { get; set; } // Optional: Low, Medium, High, Urgent
    public DateTime? Deadline { get; set; }
    public string? Instructions { get; set; } // Client instructions
    public string? RequiredFormats { get; set; } // Comma-separated: PNG, SVG, PDF, etc.
    public string? Requirements { get; set; }
    public string? ColorPreferences { get; set; }
    public string? StylePreferences { get; set; }
    public DesignCategory? DesignCategory { get; set; }
    public DesignType? DesignType { get; set; }
}
