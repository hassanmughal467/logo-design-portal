using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class LogoOrderConfiguration : IEntityTypeConfiguration<LogoOrder>
{
    public void Configure(EntityTypeBuilder<LogoOrder> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(OrderStatus.Pending);

        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Requirements)
            .HasMaxLength(2000);

        builder.Property(e => e.ColorPreferences)
            .HasMaxLength(500);

        builder.Property(e => e.StylePreferences)
            .HasMaxLength(500);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Orders)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Designer)
            .WithMany(d => d.AssignedOrders)
            .HasForeignKey(e => e.DesignerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Invoice)
            .WithOne(i => i.Order)
            .HasForeignKey<Invoice>(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
