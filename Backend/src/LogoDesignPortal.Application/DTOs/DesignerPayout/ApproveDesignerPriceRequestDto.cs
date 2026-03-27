using System.ComponentModel.DataAnnotations;

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

    /// <summary>Optional message to the designer (shown in payout negotiation history).</summary>
    [MaxLength(2000)]
    public string? Message { get; set; }
}

public enum DesignerPriceApprovalAction
{
    Approve,
    Modify,
    Reject
}
