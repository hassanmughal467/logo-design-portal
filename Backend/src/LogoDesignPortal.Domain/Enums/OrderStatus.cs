namespace LogoDesignPortal.Domain.Enums;

/// <summary>
/// Order lifecycle: WaitingForAdminApproval → PriceApprovalPending → InProgress →
/// PreviewDelivered → RevisionRequested → ClientApproved → Completed
/// </summary>
public enum OrderStatus
{
    WaitingForAdminApproval = 1,
    PriceApprovalPending = 2,
    InProgress = 3,
    PreviewDelivered = 4,
    RevisionRequested = 5,
    /// <summary>Client approved design; Admin reviews before marking Completed.</summary>
    ClientApproved = 6,
    Completed = 7,
    Cancelled = 8,
    CancelledByUser = 13,
    CancelledByAdmin = 14,
    Refunded = 15,
}
