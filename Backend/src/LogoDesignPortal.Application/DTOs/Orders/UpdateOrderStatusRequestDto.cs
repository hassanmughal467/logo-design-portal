using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class UpdateOrderStatusRequestDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
