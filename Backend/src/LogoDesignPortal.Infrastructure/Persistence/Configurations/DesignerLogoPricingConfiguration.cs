using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignerLogoPricingConfiguration : IEntityTypeConfiguration<DesignerLogoPricing>
{
    public void Configure(EntityTypeBuilder<DesignerLogoPricing> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DesignerId)
            .IsRequired();

        builder.Property(e => e.DesignCategory)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.DesignType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.DefaultPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(e => e.Designer)
            .WithMany()
            .HasForeignKey(e => e.DesignerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.DesignerId, e.DesignCategory, e.DesignType })
            .IsUnique();
    }
}
