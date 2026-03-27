using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogoDesignPortal.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.LogoName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.AttachmentsJson)
            .HasColumnType("longtext");

        builder.Property(q => q.RequestedBudget)
            .HasPrecision(18, 2);

        builder.Property(q => q.AdminQuotedPrice)
            .HasPrecision(18, 2);

        builder.Property(q => q.AdminNotes)
            .HasMaxLength(2000);

        builder.Property(q => q.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(QuoteStatus.Pending);

        builder.HasOne(q => q.Client)
            .WithMany(c => c.Quotes)
            .HasForeignKey(q => q.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.ConvertedOrder)
            .WithOne(o => o.Quote)
            .HasForeignKey<LogoOrder>(o => o.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(q => new { q.ClientId, q.Status });
        builder.HasIndex(q => q.CreatedAt);
    }
}
