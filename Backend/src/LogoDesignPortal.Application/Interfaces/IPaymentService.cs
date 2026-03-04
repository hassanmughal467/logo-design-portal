using LogoDesignPortal.Application.DTOs.Payments;

namespace LogoDesignPortal.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId);
    Task<PaymentLinkResponseDto> GeneratePaymentLinkAsync(Guid invoiceId, string paymentMethod, Guid userId);
    Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto request, Guid userId);
    Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid paymentId);
    Task<List<PaymentResponseDto>> GetPaymentsByInvoiceAsync(Guid invoiceId);
    Task<BankDetailsResponseDto> GetBankDetailsAsync();
    Task<bool> VerifyPayPalPaymentAsync(string orderId, string paymentId);
    Task<bool> VerifyWisePaymentAsync(string transferId);
    Task<PaymentResponseDto> UpdatePaymentStatusAsync(Guid paymentId, string status, string? transactionId = null);
}
