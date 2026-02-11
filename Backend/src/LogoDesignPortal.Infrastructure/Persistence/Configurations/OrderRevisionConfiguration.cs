using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class OrderRevisionConfiguration : IEntityTypeConfiguration<OrderRevision>
{
    public void Configure(EntityTypeBuilder<OrderRevision> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Instructions)
            .IsRequired()
            .HasMaxLength(5000);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Revisions)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
