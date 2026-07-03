using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotent runtime seeding for roles, permissions, and role-permission links (dev / repair CLI).
/// </summary>
public static class DatabaseStartupSeeder
{
    public static async Task EnsureRolesPermissionsAndLinksAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await EnsureStandardRolesAsync(context, logger, cancellationToken).ConfigureAwait(false);
        await EnsureDefaultPermissionsAsync(context, logger, cancellationToken).ConfigureAwait(false);
        await EnsureDefaultRolePermissionsAsync(context, logger, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureStandardRolesAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        var standardRoles = new (Guid Id, string Name, string Description)[]
        {
            (SeededRoleIds.SuperAdmin, "SuperAdmin", "Full system access with all permissions"),
            (SeededRoleIds.Admin, "Admin", "Administrative access with restricted client data access"),
            (SeededRoleIds.Designer, "Designer", "Designer access without client identity information"),
            (SeededRoleIds.Client, "Client", "Client access to their own data")
        };

        foreach (var (id, name, description) in standardRoles)
        {
            var byId = await context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct).ConfigureAwait(false);
            if (byId != null)
            {
                if (byId.IsDeleted)
                {
                    byId.IsDeleted = false;
                    byId.DeletedAt = null;
                    byId.DeletedBy = null;
                    await context.SaveChangesAsync(ct).ConfigureAwait(false);
                    logger.LogInformation("Restored soft-deleted role {RoleName}.", name);
                }
                continue;
            }

            var byName = await context.Roles.FirstOrDefaultAsync(r => r.Name == name, ct).ConfigureAwait(false);
            if (byName != null)
            {
                if (byName.IsDeleted)
                {
                    byName.IsDeleted = false;
                    byName.DeletedAt = null;
                    byName.DeletedBy = null;
                    await context.SaveChangesAsync(ct).ConfigureAwait(false);
                    logger.LogInformation("Restored soft-deleted role {RoleName} (matched by name).", name);
                }
                else
                {
                    logger.LogWarning("Role {RoleName} exists with non-standard Id {RoleId}. Skipping insert by reserved id.", name, byName.Id);
                }

                continue;
            }

            context.Roles.Add(new Role
            {
                Id = id,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            logger.LogInformation("Seeded standard role {RoleName}.", name);
        }
    }

    private static async Task EnsureDefaultPermissionsAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        foreach (var seed in PermissionSeedData.CreatePermissions())
        {
            var existing = await context.Permissions
                .FirstOrDefaultAsync(p => p.Id == seed.Id || p.Name == seed.Name, ct)
                .ConfigureAwait(false);

            if (existing == null)
            {
                context.Permissions.Add(new Permission
                {
                    Id = seed.Id,
                    Name = seed.Name,
                    Description = seed.Description,
                    Resource = seed.Resource,
                    Action = seed.Action,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                continue;
            }

            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
                existing.Name = seed.Name;
                existing.Description = seed.Description;
                existing.Resource = seed.Resource;
                existing.Action = seed.Action;
            }
        }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Default permissions ensured ({Count} seed definitions).", PermissionSeedData.CreatePermissions().Length);
    }

    private static async Task EnsureDefaultRolePermissionsAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct)
    {
        var permissionIds = await context.Permissions
            .Where(p => !p.IsDeleted)
            .ToDictionaryAsync(p => p.Name, p => p.Id, ct)
            .ConfigureAwait(false);

        var rolePermissions = new Dictionary<Guid, string[]>
        {
            [SeededRoleIds.Client] = new[] { "CreateOrder", "UploadFile", "DownloadFile", "UpdateOrderStatus" },
            [SeededRoleIds.Designer] = new[] { "UploadFile", "DownloadFile", "UpdateOrderStatus" },
            [SeededRoleIds.Admin] = new[]
            {
                "CreateUser", "ViewUsers", "ViewAllOrders", "AssignOrder", "UpdateOrderStatus",
                "CreateDesignerProfile", "ViewDesignerProfiles", "UploadFile", "DownloadFile", "DeleteFile"
            }
        };

        foreach (var (roleId, names) in rolePermissions)
        {
            foreach (var name in names)
            {
                if (!permissionIds.TryGetValue(name, out var permissionId))
                {
                    continue;
                }

                var exists = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted, ct)
                    .ConfigureAwait(false);
                if (exists)
                {
                    continue;
                }

                context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Default role permissions ensured for Client, Designer, and Admin.");
    }
}
