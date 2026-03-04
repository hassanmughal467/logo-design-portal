using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(e => e.PaymentLink)
            .HasMaxLength(1000);

        builder.Property(e => e.TransactionId)
            .HasMaxLength(200);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.ReturnUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CancelUrl)
            .HasMaxLength(500);

        builder.HasOne(e => e.Invoice)
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.InvoiceId);
        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
    }
}
