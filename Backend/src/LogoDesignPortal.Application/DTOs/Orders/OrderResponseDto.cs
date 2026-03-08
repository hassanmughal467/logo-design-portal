using LogoDesignPortal.Application.DTOs.Users;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public decimal Price { get; set; }
    public decimal? ProposedPrice { get; set; }
    public bool RequiresPriceApproval { get; set; }
    public bool PriceApproved { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Instructions { get; set; }
    public string? RequiredFormats { get; set; }
    public string? Requirements { get; set; }
    public string? ColorPreferences { get; set; }
    public string? StylePreferences { get; set; }
    public ClientInfoDto? Client { get; set; }
    public DesignerInfoDto? Designer { get; set; }
    /// <summary>For Client view: masked display when designer is assigned (e.g. "Company Design Team"). Admin/SuperAdmin see full Designer.</summary>
    public string? AssignedDesignerDisplayName { get; set; }
    public int FileCount { get; set; }
    public int VisibleFileCount { get; set; } // Files visible to current user
    public int RevisionCount { get; set; }
    public int CommentCount { get; set; }
    
    // Cancellation fields
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsCancelledByUser { get; set; }
    
    // Archive fields
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    // Refund fields
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    
    // Upload control fields
    public bool AllowUploads { get; set; } = true;
    
    // Invoice fields
    public bool HasInvoice { get; set; }
}
