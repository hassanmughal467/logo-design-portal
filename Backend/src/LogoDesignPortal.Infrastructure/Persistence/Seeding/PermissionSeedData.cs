using LogoDesignPortal.Domain.Entities;

namespace LogoDesignPortal.Infrastructure.Persistence.Seeding;

/// <summary>
/// Canonical permission rows (ids + names) used by EF migrations and runtime DB initialization.
/// </summary>
public static class PermissionSeedData
{
    public static Permission[] CreatePermissions()
    {
        var createdAt = DateTime.UtcNow;
        return new[]
        {
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "CreateUser",
                Description = "Create new users",
                Resource = "User",
                Action = "Create",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "ViewUsers",
                Description = "View all users",
                Resource = "User",
                Action = "Read",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "UpdateUser",
                Description = "Update user information",
                Resource = "User",
                Action = "Update",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "DeleteUser",
                Description = "Delete users",
                Resource = "User",
                Action = "Delete",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Name = "ManageRoles",
                Description = "Create and manage application roles (SuperAdmin role is reserved)",
                Resource = "Role",
                Action = "Manage",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "CreateDesignerProfile",
                Description = "Create designer profiles",
                Resource = "DesignerProfile",
                Action = "Create",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "ViewDesignerProfiles",
                Description = "View designer profiles",
                Resource = "DesignerProfile",
                Action = "Read",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "UpdateDesignerProfile",
                Description = "Update designer profiles",
                Resource = "DesignerProfile",
                Action = "Update",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Name = "CreateOrder",
                Description = "Create new orders",
                Resource = "Order",
                Action = "Create",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Name = "ViewAllOrders",
                Description = "View all orders in the system",
                Resource = "Order",
                Action = "ReadAll",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Name = "AssignOrder",
                Description = "Assign orders to designers",
                Resource = "Order",
                Action = "Assign",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                Name = "UpdateOrderStatus",
                Description = "Update order status",
                Resource = "Order",
                Action = "UpdateStatus",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Name = "ManagePermissions",
                Description = "Manage role permissions",
                Resource = "Permission",
                Action = "Manage",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Name = "UploadFile",
                Description = "Upload files",
                Resource = "File",
                Action = "Upload",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Name = "DownloadFile",
                Description = "Download files",
                Resource = "File",
                Action = "Download",
                CreatedAt = createdAt
            },
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                Name = "DeleteFile",
                Description = "Delete files",
                Resource = "File",
                Action = "Delete",
                CreatedAt = createdAt
            }
        };
    }
}
