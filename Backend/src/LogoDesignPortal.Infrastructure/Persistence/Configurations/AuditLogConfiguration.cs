using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PreviousValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.NewValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.PerformedByRole)
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.PerformedByUserId);
        builder.HasIndex(e => e.Timestamp);
    }
}
