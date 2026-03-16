using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignPricingConfiguration : IEntityTypeConfiguration<DesignPricing>
{
    public void Configure(EntityTypeBuilder<DesignPricing> builder)
    {
        builder.HasKey(e => e.Id);

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

        builder.HasIndex(e => new { e.DesignCategory, e.DesignType }).IsUnique();
    }
}
