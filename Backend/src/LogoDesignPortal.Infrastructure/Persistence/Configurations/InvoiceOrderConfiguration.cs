using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class InvoiceOrderConfiguration : IEntityTypeConfiguration<InvoiceOrder>
{
    public void Configure(EntityTypeBuilder<InvoiceOrder> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.InvoiceOrders)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index for (InvoiceId, OrderId) - MySQL doesn't support filtered indexes
        builder.HasIndex(e => new { e.InvoiceId, e.OrderId })
            .IsUnique();
    }
}
