namespace LogoDesignPortal.Domain.Enums;

public enum OrderStatus
{
    // Original statuses (keeping for backward compatibility)
    WaitingForAdminApproval = 1,
    PriceApprovalPending = 2,
    InProgress = 3,
    PreviewDelivered = 4,
    RevisionRequested = 5,
    FinalApproved = 6,
    Completed = 7,
    Cancelled = 8,
    
    // New professional statuses
    Pending = 10,
    Paid = 11,
    Processing = 12,
    CancelledByUser = 13,
    CancelledByAdmin = 14,
    Refunded = 15,
    Failed = 16,
    Archived = 17
}
