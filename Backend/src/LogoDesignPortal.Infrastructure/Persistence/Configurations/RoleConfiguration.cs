using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Seed initial roles
        SeedRoles(builder);
    }

    private void SeedRoles(EntityTypeBuilder<Role> builder)
    {
        var roles = new[]
        {
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "SuperAdmin",
                Description = "Full system access with all permissions",
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Admin",
                Description = "Administrative access with restricted client data access",
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Designer",
                Description = "Designer access without client identity information",
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Client",
                Description = "Client access to their own data",
                CreatedAt = DateTime.UtcNow
            }
        };

        builder.HasData(roles);
    }
}
