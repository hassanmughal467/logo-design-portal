using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class CreateManualCompletedOrderRequestDto
{
    public Guid? ClientUserId { get; set; }

    public NewManualClientDto? NewClient { get; set; }

    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public Guid DesignerUserId { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    public DateTime CompletedAt { get; set; }

    public string? Notes { get; set; }
}

public class NewManualClientDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;
}
