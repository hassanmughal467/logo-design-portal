using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Quote : BaseEntity
{
    public Guid ClientId { get; set; }
    public string LogoName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public decimal? RequestedBudget { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal? AdminQuotedPrice { get; set; }
    public string? AdminNotes { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Pending;
    public Guid? ConvertedOrderId { get; set; }

    public ClientProfile Client { get; set; } = null!;
    public LogoOrder? ConvertedOrder { get; set; }
}
