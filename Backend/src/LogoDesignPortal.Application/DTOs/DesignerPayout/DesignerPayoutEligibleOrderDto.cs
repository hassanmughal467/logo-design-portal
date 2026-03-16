namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// DTO for orders eligible for designer payout, used by the Invoice Builder grid.
/// </summary>
public class DesignerPayoutEligibleOrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? DesignCategory { get; set; }
    public string? DesignType { get; set; }
    public decimal ApprovedPrice { get; set; }
    public DateTime? CompletedDate { get; set; }
    /// <summary>URL path for preview image from ClientGallery (e.g. /api/files/{fileId}/download).</summary>
    public string? PreviewImageUrl { get; set; }
}
