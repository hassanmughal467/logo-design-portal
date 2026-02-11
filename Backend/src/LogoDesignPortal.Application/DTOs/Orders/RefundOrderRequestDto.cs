using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class RefundOrderRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MinLength(5)]
    [MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;
}
