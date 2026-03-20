using LogoDesignPortal.Application.DTOs.Permissions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description ?? string.Empty
        }).ToList();
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action
        }).ToList();
    }

    public async Task<RolePermissionsResponseDto> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        return new RolePermissionsResponseDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Permissions = role.RolePermissions
                .Where(rp => !rp.IsDeleted && !rp.Permission.IsDeleted)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action
                }).ToList()
        };
    }

    public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == permissionId && !p.IsDeleted);

        if (permission == null)
        {
            throw new InvalidOperationException("Permission not found.");
        }

        // Check if already assigned
        var existing = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

        if (existing != null)
        {
            throw new InvalidOperationException("Permission is already assigned to this role.");
        }

        var rolePermission = new Domain.Entities.RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task RevokePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

        if (rolePermission == null)
        {
            throw new InvalidOperationException("Permission is not assigned to this role.");
        }

        // Soft delete
        rolePermission.IsDeleted = true;
        rolePermission.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == null)
        {
            return false;
        }

        // SuperAdmin has all permissions
        if (user.Role.Name == "SuperAdmin")
        {
            return true;
        }

        return user.Role.RolePermissions
            .Any(rp => !rp.IsDeleted && 
                       !rp.Permission.IsDeleted && 
                       rp.Permission.Name == permissionName);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == null)
        {
            return new List<string>();
        }

        // SuperAdmin has all permissions (return special marker or all permissions)
        if (user.Role.Name == "SuperAdmin")
        {
            return new List<string> { "*" }; // "*" means all permissions
        }

        return user.Role.RolePermissions
            .Where(rp => !rp.IsDeleted && !rp.Permission.IsDeleted)
            .Select(rp => rp.Permission.Name)
            .ToList();
    }
}
