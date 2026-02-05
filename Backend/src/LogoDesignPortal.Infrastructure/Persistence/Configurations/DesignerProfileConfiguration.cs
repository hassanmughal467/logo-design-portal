using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class DesignerProfileConfiguration : IEntityTypeConfiguration<DesignerProfile>
{
    public void Configure(EntityTypeBuilder<DesignerProfile> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Specialization)
            .HasMaxLength(200);

        builder.Property(e => e.Bio)
            .HasMaxLength(1000);

        builder.Property(e => e.HourlyRate)
            .HasPrecision(18, 2);

        builder.Property(e => e.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(e => e.User)
            .WithOne(u => u.DesignerProfile)
            .HasForeignKey<DesignerProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
