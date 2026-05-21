using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.InvoiceNumber).IsUnique();

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(InvoiceStatus.Pending);

        builder.Property(e => e.BillingType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BillingType.PerLogo); // Default for backward compatibility

        builder.Property(e => e.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.BillingPeriod)
            .HasMaxLength(100);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Financial analytics indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.PaidDate);
        builder.HasIndex(e => new { e.ClientId, e.Status });
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => new { e.Status, e.DueDate });

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
