namespace LogoDesignPortal.Application.DTOs.Payments;

public class ProcessPaymentRequestDto
{
    public Guid PaymentId { get; set; }
    public string? PayPalOrderId { get; set; }
    public string? WiseTransferId { get; set; }
    public string? BankReference { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}
