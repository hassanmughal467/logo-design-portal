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
    public decimal Price { get; set; }
    public decimal? ProposedPrice { get; set; } // Price proposed by admin
    public bool RequiresPriceApproval { get; set; } = false;
    public bool PriceApproved { get; set; } = false;
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

    // Navigation properties
    public ClientProfile Client { get; set; } = null!;
    public DesignerProfile? Designer { get; set; }
    public ICollection<LogoFile> Files { get; set; } = new List<LogoFile>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<OrderRevision> Revisions { get; set; } = new List<OrderRevision>();
    public ICollection<OrderComment> Comments { get; set; } = new List<OrderComment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
