using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    public void Configure(EntityTypeBuilder<ClientProfile> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BillingType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BillingType.PerLogo);

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ContactName)
            .HasMaxLength(200);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(e => e.Cell)
            .HasMaxLength(20);

        builder.Property(e => e.Fax)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.State)
            .HasMaxLength(100);

        builder.Property(e => e.Country)
            .HasMaxLength(100);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);

        builder.Property(e => e.Website)
            .HasMaxLength(500);

        builder.Property(e => e.Reference)
            .HasMaxLength(200);

        builder.Property(e => e.CustomerType)
            .HasConversion<int>()
            .IsRequired(false);

        builder.HasOne(e => e.User)
            .WithOne(u => u.ClientProfile)
            .HasForeignKey<ClientProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
