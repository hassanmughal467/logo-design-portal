using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(NotificationType.Info);

        builder.Property(e => e.ReferenceType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(NotificationReferenceType.Order);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Notifications)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(e => e.AggregationCount)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.LastOccurrenceAt);

        builder.HasIndex(e => new { e.UserId, e.IsRead });
        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => new { e.UserId, e.ReferenceType, e.CreatedAt });
    }
}
