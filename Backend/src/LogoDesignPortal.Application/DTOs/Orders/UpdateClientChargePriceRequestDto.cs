using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

/// <summary>
/// Request to update the client charge price for an order. Admin only.
/// </summary>
public class UpdateClientChargePriceRequestDto
{
    [Required]
    [Range(0, 1000000, ErrorMessage = "Client charge price must be between 0 and 1,000,000.")]
    public decimal ClientChargePrice { get; set; }
}
