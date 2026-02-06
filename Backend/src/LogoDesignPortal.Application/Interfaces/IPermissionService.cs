using LogoDesignPortal.Application.DTOs.Permissions;

namespace LogoDesignPortal.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<RolePermissionsResponseDto> GetRolePermissionsAsync(Guid roleId);
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task RevokePermissionFromRoleAsync(Guid roleId, Guid permissionId);
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}
