using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // PayPal, Wise, BankTransfer
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? PaymentLink { get; set; }
    public string? TransactionId { get; set; } // PayPal Order ID, Wise Transfer ID, Bank Reference
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
}
