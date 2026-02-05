using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class AssignOrderRequestDto
{
    [Required]
    public Guid DesignerId { get; set; }
}
