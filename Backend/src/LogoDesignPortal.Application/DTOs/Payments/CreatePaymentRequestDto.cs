namespace LogoDesignPortal.Application.DTOs.Payments;

public class CreatePaymentRequestDto
{
    public Guid InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // PayPal, Wise, BankTransfer
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}
