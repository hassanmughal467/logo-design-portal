namespace LogoDesignPortal.Application.DTOs.Payments;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed, Cancelled
    public string? PaymentLink { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
