using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignerInvoiceAdjustmentConfiguration : IEntityTypeConfiguration<DesignerInvoiceAdjustment>
{
    public void Configure(EntityTypeBuilder<DesignerInvoiceAdjustment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(e => e.DesignerInvoice)
            .WithMany(i => i.Adjustments)
            .HasForeignKey(e => e.DesignerInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
