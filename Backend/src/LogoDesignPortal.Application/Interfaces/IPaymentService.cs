using LogoDesignPortal.Application.DTOs.Payments;

namespace LogoDesignPortal.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId);
    Task<PaymentLinkResponseDto> GeneratePaymentLinkAsync(Guid invoiceId, string paymentMethod, Guid userId);
    Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto request, Guid userId);
    Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid paymentId);
    /// <summary>
    /// Gets a payment by ID with access control. Returns null if not found or user lacks access.
    /// </summary>
    Task<PaymentResponseDto?> GetPaymentByIdWithAccessAsync(Guid paymentId, Guid userId, string? userRole);
    Task<List<PaymentResponseDto>> GetPaymentsByInvoiceAsync(Guid invoiceId);
    /// <summary>
    /// Gets payments by invoice with access control. Returns null if user lacks access to the invoice.
    /// </summary>
    Task<List<PaymentResponseDto>?> GetPaymentsByInvoiceWithAccessAsync(Guid invoiceId, Guid userId, string? userRole);
    Task<BankDetailsResponseDto> GetBankDetailsAsync();
    Task<bool> VerifyPayPalPaymentAsync(string orderId, string paymentId);
    Task<bool> VerifyWisePaymentAsync(string transferId);
    Task<PaymentResponseDto> UpdatePaymentStatusAsync(Guid paymentId, string status, string? transactionId = null);
    /// <summary>
    /// Verifies a PayPal webhook signature. Returns true if valid.
    /// </summary>
    Task<bool> VerifyPayPalWebhookSignatureAsync(string transmissionId, string transmissionTime, string transmissionSig, string authAlgo, string certUrl, string webhookEventJson);
}
