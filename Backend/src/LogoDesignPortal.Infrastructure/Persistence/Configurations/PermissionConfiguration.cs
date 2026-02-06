using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Resource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);

        // Seed initial permissions
        SeedPermissions(builder);
    }

    private void SeedPermissions(EntityTypeBuilder<Permission> builder)
    {
        var permissions = new[]
        {
            // User Management Permissions
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "CreateUser",
                Description = "Create new users",
                Resource = "User",
                Action = "Create",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "ViewUsers",
                Description = "View all users",
                Resource = "User",
                Action = "Read",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "UpdateUser",
                Description = "Update user information",
                Resource = "User",
                Action = "Update",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "DeleteUser",
                Description = "Delete users",
                Resource = "User",
                Action = "Delete",
                CreatedAt = DateTime.UtcNow
            },

            // Designer Profile Permissions
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "CreateDesignerProfile",
                Description = "Create designer profiles",
                Resource = "DesignerProfile",
                Action = "Create",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "ViewDesignerProfiles",
                Description = "View designer profiles",
                Resource = "DesignerProfile",
                Action = "Read",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "UpdateDesignerProfile",
                Description = "Update designer profiles",
                Resource = "DesignerProfile",
                Action = "Update",
                CreatedAt = DateTime.UtcNow
            },

            // Order Management Permissions
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Name = "CreateOrder",
                Description = "Create new orders",
                Resource = "Order",
                Action = "Create",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Name = "ViewAllOrders",
                Description = "View all orders in the system",
                Resource = "Order",
                Action = "ReadAll",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Name = "AssignOrder",
                Description = "Assign orders to designers",
                Resource = "Order",
                Action = "Assign",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                Name = "UpdateOrderStatus",
                Description = "Update order status",
                Resource = "Order",
                Action = "UpdateStatus",
                CreatedAt = DateTime.UtcNow
            },

            // Permission Management
            new Permission
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Name = "ManagePermissions",
                Description = "Manage role permissions",
                Resource = "Permission",
                Action = "Manage",
                CreatedAt = DateTime.UtcNow
            },

            // File Management Permissions
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Name = "UploadFile",
                Description = "Upload files",
                Resource = "File",
                Action = "Upload",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Name = "DownloadFile",
                Description = "Download files",
                Resource = "File",
                Action = "Download",
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                Name = "DeleteFile",
                Description = "Delete files",
                Resource = "File",
                Action = "Delete",
                CreatedAt = DateTime.UtcNow
            }
        };

        builder.HasData(permissions);
    }
}
