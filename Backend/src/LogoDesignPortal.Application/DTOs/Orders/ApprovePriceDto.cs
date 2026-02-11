using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class ApprovePriceDto
{
    [Required]
    public bool Approved { get; set; }

    public string? Comment { get; set; }
}
