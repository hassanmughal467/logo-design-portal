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
            .HasDefaultValue(OrderStatus.WaitingForAdminApproval);

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(OrderPriority.Medium);

        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.StandardPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ProposedPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ApprovedPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ClientPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ClientBasePrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(e => e.ClientChargePrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(e => e.DesignerProposedPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.DesignerApprovedPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.PriceApprovalStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PriceApprovalStatus.NotSubmitted);

        builder.Property(e => e.DesignCategory)
            .HasConversion<int>();

        builder.Property(e => e.DesignType)
            .HasConversion<int>();

        builder.Property(e => e.IsDesignerInvoiced)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Instructions)
            .HasMaxLength(5000);

        builder.Property(e => e.RequiredFormats)
            .HasMaxLength(500);

        builder.Property(e => e.Requirements)
            .HasMaxLength(2000);

        builder.Property(e => e.ColorPreferences)
            .HasMaxLength(500);

        builder.Property(e => e.StylePreferences)
            .HasMaxLength(500);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(2000);

        builder.Property(e => e.RefundReason)
            .HasMaxLength(2000);

        builder.Property(e => e.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.AllowUploads)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.RevisionCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.RevisionLimit)
            .IsRequired(false);

        builder.Property(e => e.AllowExtraRevisions)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsInvoiced)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.BillingEligible)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.OrderSource)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(OrderSource.Portal);

        builder.Property(e => e.QuoteId)
            .IsRequired(false);

        builder.Property(e => e.PriceUpdatedByRole)
            .HasMaxLength(50);

        builder.Property(e => e.PriceUpdatedAt);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Orders)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Designer)
            .WithMany(d => d.AssignedOrders)
            .HasForeignKey(e => e.DesignerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.DesignerInvoice)
            .WithMany()
            .HasForeignKey(e => e.DesignerInvoiceId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(e => new { e.ClientId, e.BillingEligible, e.IsInvoiced });
        builder.HasIndex(e => new { e.Status, e.BillingEligible, e.IsInvoiced });
        builder.HasIndex(e => e.CompletedDate);
        builder.HasIndex(e => e.Deadline);
        builder.HasIndex(e => new { e.DesignerId, e.IsDesignerInvoiced });

        // Analytics indexes for optimized aggregation queries
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.UpdatedAt);
        builder.HasIndex(e => new { e.Status, e.CreatedAt });
        // Revenue / completion-time analytics (filter Completed + range on completion timestamp)
        builder.HasIndex(e => new { e.Status, e.UpdatedAt });
        builder.HasIndex(e => new { e.ClientId, e.CreatedAt });
        builder.HasIndex(e => new { e.ClientId, e.Status });
        builder.HasIndex(e => new { e.DesignerId, e.Status });
        builder.HasIndex(e => new { e.IsArchived, e.CreatedAt });
        builder.HasIndex(e => e.QuoteId).IsUnique();

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
