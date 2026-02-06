namespace LogoDesignPortal.Application.DTOs.Permissions;

public class RolePermissionsResponseDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}
