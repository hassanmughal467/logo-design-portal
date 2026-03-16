namespace LogoDesignPortal.Domain.Enums;

/// <summary>
/// Status of designer price approval workflow.
/// </summary>
public enum PriceApprovalStatus
{
    NotSubmitted = 0,
    PendingApproval = 1,
    AutoApproved = 2,
    Approved = 3,
    Modified = 4,
    Rejected = 5
}
