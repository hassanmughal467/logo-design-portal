namespace LogoDesignPortal.Application.DTOs.Orders;

/// <summary>
/// Optional payload for the Approve Order endpoint.
/// - When <see cref="DesignerId"/> is null the order moves to <c>ApprovedUnassigned</c>
///   and surfaces in the SuperAdmin Unassigned Orders alert widget.
/// - When <see cref="DesignerId"/> is provided the order is approved and assigned in one step,
///   moving directly to <c>InProgress</c>.
/// </summary>
public class ApproveOrderRequestDto
{
    /// <summary>Optional designer User ID. When supplied, the order is also assigned in the same operation.</summary>
    public Guid? DesignerId { get; set; }

    /// <summary>Optional approval note recorded in order status history.</summary>
    public string? Notes { get; set; }
}
