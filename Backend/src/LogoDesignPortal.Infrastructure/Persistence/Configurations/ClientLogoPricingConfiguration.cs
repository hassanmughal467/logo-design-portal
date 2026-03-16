using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class ClientLogoPricingConfiguration : IEntityTypeConfiguration<ClientLogoPricing>
{
    public void Configure(EntityTypeBuilder<ClientLogoPricing> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClientId)
            .IsRequired();

        builder.Property(e => e.DesignCategory)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.DesignType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ClientId, e.DesignCategory, e.DesignType })
            .IsUnique();
    }
}
