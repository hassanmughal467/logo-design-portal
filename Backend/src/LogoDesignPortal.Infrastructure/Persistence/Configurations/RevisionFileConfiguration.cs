using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class RevisionFileConfiguration : IEntityTypeConfiguration<RevisionFile>
{
    public void Configure(EntityTypeBuilder<RevisionFile> builder)
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
            .HasDefaultValue(RevisionFileType.ReferenceImage);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Revision)
            .WithMany(r => r.Files)
            .HasForeignKey(e => e.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
