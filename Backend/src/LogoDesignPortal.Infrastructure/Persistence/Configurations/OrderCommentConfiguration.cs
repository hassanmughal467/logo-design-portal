using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class OrderCommentConfiguration : IEntityTypeConfiguration<OrderComment>
{
    public void Configure(EntityTypeBuilder<OrderComment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.CommentType)
            .HasConversion<int>()
            .HasDefaultValue(CommentType.General);

        builder.Property(e => e.VisibleToClient)
            .HasDefaultValue(false);

        builder.Property(e => e.IsReadByClient)
            .HasDefaultValue(false);

        builder.Property(e => e.IsReadByDesigner)
            .HasDefaultValue(false);

        builder.Property(e => e.IsReadByAdmin)
            .HasDefaultValue(false);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Comments)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
