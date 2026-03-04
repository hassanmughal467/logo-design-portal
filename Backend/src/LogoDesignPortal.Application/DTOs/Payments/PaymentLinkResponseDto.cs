namespace LogoDesignPortal.Application.DTOs.Payments;

public class PaymentLinkResponseDto
{
    public string PaymentLink { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? QrCode { get; set; } // Base64 encoded QR code
}
