using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class OrderLogConfiguration : IEntityTypeConfiguration<OrderLog>
{
    public void Configure(EntityTypeBuilder<OrderLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.PreviousStatus)
            .HasConversion<int>();

        builder.Property(e => e.NewStatus)
            .HasConversion<int>();

        builder.Property(e => e.PerformedBy)
            .HasMaxLength(100);

        builder.Property(e => e.Note)
            .HasMaxLength(2000);

        builder.Property(e => e.Metadata)
            .HasMaxLength(5000);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => e.CreatedAt);
    }
}
