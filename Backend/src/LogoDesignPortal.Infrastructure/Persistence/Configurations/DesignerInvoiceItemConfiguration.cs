using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignerInvoiceItemConfiguration : IEntityTypeConfiguration<DesignerInvoiceItem>
{
    public void Configure(EntityTypeBuilder<DesignerInvoiceItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(e => e.DesignerInvoice)
            .WithMany(i => i.Items)
            .HasForeignKey(e => e.DesignerInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.DesignerInvoiceId, e.OrderId }).IsUnique();
    }
}
