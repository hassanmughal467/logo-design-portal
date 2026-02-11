using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class InvoiceLogConfiguration : IEntityTypeConfiguration<InvoiceLog>
{
    public void Configure(EntityTypeBuilder<InvoiceLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.InvoiceLogs)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.InvoiceId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
