namespace LogoDesignPortal.Application.DTOs.Quotes;

public class QuoteResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string LogoName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new();
    public decimal? RequestedBudget { get; set; }
    public decimal? AdminQuotedPrice { get; set; }
    public string? AdminNotes { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public Guid? ConvertedOrderId { get; set; }
}
