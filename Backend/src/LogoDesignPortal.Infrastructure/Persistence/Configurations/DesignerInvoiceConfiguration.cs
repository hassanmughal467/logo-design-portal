using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignerInvoiceConfiguration : IEntityTypeConfiguration<DesignerInvoice>
{
    public void Configure(EntityTypeBuilder<DesignerInvoice> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.InvoiceNumber).IsUnique();

        builder.Property(e => e.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DesignerInvoiceStatus.Pending);

        builder.Property(e => e.BillingPeriod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Designer)
            .WithMany()
            .HasForeignKey(e => e.DesignerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.DesignerInvoice)
            .HasForeignKey(i => i.DesignerInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Adjustments)
            .WithOne(a => a.DesignerInvoice)
            .HasForeignKey(a => a.DesignerInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.DesignerId, e.BillingPeriod });
    }
}
