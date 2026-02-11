using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class RequestPriceApprovalDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal ProposedPrice { get; set; }

    public string? Notes { get; set; }
}
