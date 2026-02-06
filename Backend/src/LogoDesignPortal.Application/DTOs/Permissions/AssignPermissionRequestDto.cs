using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Permissions;

public class AssignPermissionRequestDto
{
    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid PermissionId { get; set; }
}
