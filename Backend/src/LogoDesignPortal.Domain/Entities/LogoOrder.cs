using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class LogoOrder : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid? DesignerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.WaitingForAdminApproval;
    public OrderPriority Priority { get; set; } = OrderPriority.Medium;
    /// <summary>Legacy/display price. Prefer ClientChargePrice for client invoicing.</summary>
    public decimal Price { get; set; }
    /// <summary>Client-facing price (from ClientLogoPricing or default). Used for invoicing. Deprecated: use ClientChargePrice.</summary>
    public decimal? ClientPrice { get; set; }
    /// <summary>Base price from ClientLogoPricing at order creation. Never changes.</summary>
    public decimal ClientBasePrice { get; set; }
    /// <summary>Actual price charged to client. Defaults to ClientBasePrice; admin may adjust.</summary>
    public decimal ClientChargePrice { get; set; }
    /// <summary>Currency for client price (e.g. USD, PKR, EUR).</summary>
    public string CurrencyCode { get; set; } = "USD";
    /// <summary>Standard/default payout price (PKR) from DesignPricing. Set when designer submits pricing.</summary>
    public decimal? StandardPrice { get; set; }
    /// <summary>Designer's proposed payout price (PKR). Set when designer submits complexity request.</summary>
    public decimal? DesignerProposedPrice { get; set; }
    /// <summary>Admin-approved designer payout price (PKR). Used for designer invoice.</summary>
    public decimal? DesignerApprovedPrice { get; set; }
    /// <summary>Legacy: Designer's proposed payout. Use DesignerProposedPrice.</summary>
    public decimal? ProposedPrice { get; set; }
    /// <summary>Legacy: Admin-approved designer payout. Use DesignerApprovedPrice.</summary>
    public decimal? ApprovedPrice { get; set; }
    /// <summary>Status of the designer price approval workflow.</summary>
    public PriceApprovalStatus PriceApprovalStatus { get; set; } = PriceApprovalStatus.NotSubmitted;
    /// <summary>True when proposed price differs from standard; requires admin approval.</summary>
    public bool RequiresPriceApproval { get; set; } = false;
    /// <summary>True when admin has approved the price (ApprovedPrice is final).</summary>
    public bool PriceApproved { get; set; } = false;

    // Designer payout: design category and type (required when designer uploads final files)
    public DesignCategory? DesignCategory { get; set; }
    public DesignType? DesignType { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Instructions { get; set; } // Client instructions
    public string? RequiredFormats { get; set; } // Comma-separated: PNG, SVG, PDF, etc.
    public string? Requirements { get; set; }
    public string? ColorPreferences { get; set; }
    public string? StylePreferences { get; set; }

    // Cancellation fields
    public string? CancellationReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsCancelledByUser { get; set; } = false;

    // Archive field
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    // Refund fields
    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }
    public Guid? RefundedBy { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }

    // Upload control fields
    public bool AllowUploads { get; set; } = true; // Admin can disable uploads for completed orders

    // Client charge price audit (who last changed the price)
    public string? PriceUpdatedByRole { get; set; }
    public Guid? PriceUpdatedByUserId { get; set; }
    public DateTime? PriceUpdatedAt { get; set; }

    // Billing queue fields (client invoices)
    public bool IsInvoiced { get; set; }
    public Guid? InvoiceId { get; set; }

    // Designer payout fields
    public bool IsDesignerInvoiced { get; set; }
    public Guid? DesignerInvoiceId { get; set; }
    public DateTime? InvoicedDate { get; set; }
    public bool BillingEligible { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Revision tracking (Fiverr/Upwork style)
    /// <summary>Number of times the client has requested a revision. Incremented on each RequestRevision.</summary>
    public int RevisionCount { get; set; }
    /// <summary>Max revisions allowed for this package. Null = unlimited.</summary>
    public int? RevisionLimit { get; set; }
    /// <summary>Admin override: allow additional revisions beyond RevisionLimit.</summary>
    public bool AllowExtraRevisions { get; set; }

    // Navigation properties
    public ClientProfile Client { get; set; } = null!;
    public DesignerProfile? Designer { get; set; }
    public ICollection<LogoFile> Files { get; set; } = new List<LogoFile>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<OrderRevision> Revisions { get; set; } = new List<OrderRevision>();
    public ICollection<OrderComment> Comments { get; set; } = new List<OrderComment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public DesignerInvoice? DesignerInvoice { get; set; }
}
