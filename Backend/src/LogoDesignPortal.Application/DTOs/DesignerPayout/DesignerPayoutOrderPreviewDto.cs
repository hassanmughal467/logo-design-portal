namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

/// <summary>
/// DTO for design preview modal before payout.
/// </summary>
public class DesignerPayoutOrderPreviewDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? DesignCategory { get; set; }
    public string? DesignType { get; set; }
    public DateTime? CompletedDate { get; set; }
    /// <summary>List of file URLs from ClientGallery (e.g. /api/files/{fileId}/download).</summary>
    public List<string> FinalFiles { get; set; } = new();
}
