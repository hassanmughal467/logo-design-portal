using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class SetAllowUploadsRequestDto
{
    [Required]
    public bool AllowUploads { get; set; }
}
