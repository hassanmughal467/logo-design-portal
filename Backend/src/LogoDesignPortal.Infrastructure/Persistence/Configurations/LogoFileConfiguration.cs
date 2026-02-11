using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class LogoFileConfiguration : IEntityTypeConfiguration<LogoFile>
{
    public void Configure(EntityTypeBuilder<LogoFile> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.FileType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(FileType.Reference);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Files)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
