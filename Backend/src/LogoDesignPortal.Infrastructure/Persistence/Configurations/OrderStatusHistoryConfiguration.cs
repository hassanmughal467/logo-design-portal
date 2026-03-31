using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PreviousStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.NewStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Filter/sort analytics and order timeline queries by order + status + time.
        builder.HasIndex(e => new { e.OrderId, e.NewStatus, e.CreatedAt });
    }
}
