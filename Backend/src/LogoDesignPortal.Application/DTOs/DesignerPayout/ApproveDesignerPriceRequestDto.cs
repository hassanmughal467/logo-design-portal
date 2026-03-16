namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// Admin request to approve, modify, or reject designer's proposed price.
/// </summary>
public class ApproveDesignerPriceRequestDto
{
    /// <summary>Final approved amount (PKR). Required for Approve/Modify.</summary>
    public decimal? ApprovedPrice { get; set; }
    /// <summary>Approve, Modify, or Reject.</summary>
    public DesignerPriceApprovalAction Action { get; set; }
}

public enum DesignerPriceApprovalAction
{
    Approve,
    Modify,
    Reject
}
