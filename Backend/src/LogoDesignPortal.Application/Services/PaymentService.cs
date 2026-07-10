using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace LogoDesignPortal.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly IReadModelCacheVersions _readModelCache;
    private readonly ILogger<PaymentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentService(
        IApplicationDbContext context,
        IMapper mapper,
        ISettingsService settingsService,
        INotificationService notificationService,
        IReadModelCacheVersions readModelCache,
        ILogger<PaymentService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _mapper = mapper;
        _settingsService = settingsService;
        _notificationService = notificationService;
        _readModelCache = readModelCache;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private void InvalidateFinancialReadModels()
    {
        _readModelCache.BumpOrders();
        _readModelCache.BumpAnalytics();
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId)
    {
        await AuthorizeInvoicePaymentMutationAsync(request.InvoiceId, userId);

        // Create payment record
        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            Currency = request.Currency ?? "USD",
            Status = PaymentStatus.Pending,
            ReturnUrl = request.ReturnUrl,
            CancelUrl = request.CancelUrl,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Payment links expire in 30 days
            CreatedBy = userId
        };

        // Generate payment link based on method
        string paymentLink = string.Empty;
        string? transactionId = null;

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            throw new InvalidOperationException("Payment method is required.");

        try
        {
            switch (request.PaymentMethod.ToLower())
            {
                case "paypal":
                    var paypalResult = await CreatePayPalPaymentAsync(request);
                    paymentLink = paypalResult.PaymentLink;
                    transactionId = paypalResult.TransactionId;
                    break;

                case "wise":
                    var wiseResult = await CreateWisePaymentAsync(request);
                    paymentLink = wiseResult.PaymentLink;
                    transactionId = wiseResult.TransactionId;
                    break;

                case "banktransfer":
                case "bank":
                    // For bank transfer, we'll generate a payment link that shows bank details
                    paymentLink = await GenerateBankTransferLinkAsync(request.InvoiceId);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported payment method: {request.PaymentMethod}");
            }

            payment.PaymentLink = paymentLink;
            payment.TransactionId = transactionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for invoice {InvoiceId}", request.InvoiceId);
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = ex.Message;
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return _mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<PaymentLinkResponseDto> GeneratePaymentLinkAsync(Guid invoiceId, string paymentMethod, Guid userId)
    {
        var invoice = await AuthorizeInvoicePaymentMutationAsync(invoiceId, userId);

        var request = new CreatePaymentRequestDto
        {
            InvoiceId = invoiceId,
            PaymentMethod = paymentMethod,
            Amount = invoice.TotalAmount,
            Currency = "USD",
            ReturnUrl = $"{GetFrontendUrl()}/invoices?payment=success",
            CancelUrl = $"{GetFrontendUrl()}/invoices?payment=cancelled"
        };

        var payment = await CreatePaymentAsync(request, userId);

        return new PaymentLinkResponseDto
        {
            PaymentLink = payment.PaymentLink ?? string.Empty,
            PaymentId = payment.Id,
            InvoiceId = invoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = paymentMethod,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto request, Guid userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && !p.IsDeleted);

        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found.");
        }

        await AuthorizeInvoicePaymentMutationAsync(payment.InvoiceId, userId);

        var verified = false;
        var pendingManualVerification = false;

        try
        {
            switch (payment.PaymentMethod.ToLower())
            {
                case "paypal":
                    if (!string.IsNullOrEmpty(request.PayPalOrderId))
                    {
                        verified = await VerifyPayPalPaymentAsync(request.PayPalOrderId, request.PayPalOrderId);
                        if (verified)
                        {
                            payment.TransactionId = request.PayPalOrderId;
                        }
                    }
                    break;

                case "wise":
                    if (!string.IsNullOrEmpty(request.WiseTransferId))
                    {
                        verified = await VerifyWisePaymentAsync(request.WiseTransferId);
                        if (verified)
                        {
                            payment.TransactionId = request.WiseTransferId;
                        }
                    }
                    break;

                case "banktransfer":
                case "bank":
                    if (string.IsNullOrWhiteSpace(request.BankReference))
                    {
                        throw new InvalidOperationException("Bank reference is required for bank transfer payments.");
                    }

                    payment.TransactionId = request.BankReference.Trim();
                    payment.Status = PaymentStatus.Processing;
                    payment.CompletedAt = null;
                    payment.ErrorMessage = null;
                    pendingManualVerification = true;
                    break;
            }

            if (pendingManualVerification)
            {
                _logger.LogInformation(
                    "Bank transfer reference submitted for payment {PaymentId}; awaiting admin verification.",
                    payment.Id);
            }
            else if (verified)
            {
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = DateTime.UtcNow;
                payment.ErrorMessage = null;

                // Mark invoice as paid
                if (payment.Invoice.Status != InvoiceStatus.Paid)
                {
                    payment.Invoice.Status = InvoiceStatus.Paid;
                    payment.Invoice.PaidDate = DateTime.UtcNow;
                    payment.Invoice.PaymentMethod = payment.PaymentMethod;

                    // Notify Admin and SuperAdmin: Payment received
                    try
                    {
                        var firstOrderId = await _context.InvoiceOrders
                            .Where(io => io.InvoiceId == payment.InvoiceId && io.OrderId.HasValue)
                            .Select(io => io.OrderId)
                            .FirstOrDefaultAsync();
                        var title = "Payment Received";
                        var message = firstOrderId.HasValue
                            ? $"Payment received for order (#{NotificationFormatHelper.GetOrderNumber(firstOrderId.Value)})"
                            : $"Payment received for invoice (#{NotificationFormatHelper.GetInvoiceNumber(payment.Invoice.InvoiceNumber)})";
                        await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, payment.Invoice.Id, userId);
                        await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, payment.Invoice.Id, userId);
                    }
                    catch { /* Non-critical */ }
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = "Payment verification failed";
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", request.PaymentId);
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = ex.Message;
        }

        payment.UpdatedBy = userId;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (payment.Status == PaymentStatus.Completed)
            InvalidateFinancialReadModels();

        return _mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

        return payment == null ? null : _mapper.Map<PaymentResponseDto>(payment);
    }

    private async Task<Invoice> AuthorizeInvoicePaymentMutationAsync(Guid invoiceId, Guid userId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        var roleName = currentUser?.Role?.Name;
        if (roleName == nameof(SystemRoles.Admin) || roleName == nameof(SystemRoles.SuperAdmin))
        {
            return invoice;
        }

        if (roleName == nameof(SystemRoles.Client) && invoice.Client?.UserId == userId)
        {
            return invoice;
        }

        throw new ForbiddenAccessException("You don't have permission to create payments for this invoice.");
    }

    public async Task<PaymentResponseDto?> GetPaymentByIdWithAccessAsync(Guid paymentId, Guid userId, string? userRole)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Client)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

        if (payment == null)
        {
            return null;
        }

        // Access control: Client can only access payments for their own invoices; Admin/SuperAdmin can access all
        if (userRole == "Client" && payment.Invoice.Client.UserId != userId)
        {
            return null;
        }

        return _mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<List<PaymentResponseDto>> GetPaymentsByInvoiceAsync(Guid invoiceId)
    {
        var payments = await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<PaymentResponseDto>>(payments);
    }

    public async Task<List<PaymentResponseDto>?> GetPaymentsByInvoiceWithAccessAsync(Guid invoiceId, Guid userId, string? userRole)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return null;
        }

        // Access control: Client can only access payments for their own invoices; Admin/SuperAdmin can access all
        if (userRole == "Client" && invoice.Client.UserId != userId)
        {
            return null;
        }

        return await GetPaymentsByInvoiceAsync(invoiceId);
    }

    public async Task<BankDetailsResponseDto> GetBankDetailsAsync()
    {
        // Get bank details from settings
        var bankDetails = new BankDetailsResponseDto
        {
            BankName = await _settingsService.GetSettingAsync("BankName") ?? "Not configured",
            AccountHolderName = await _settingsService.GetSettingAsync("AccountHolderName") ?? "Not configured",
            AccountNumber = await _settingsService.GetSettingAsync("AccountNumber") ?? "Not configured",
            IBAN = await _settingsService.GetSettingAsync("IBAN"),
            SWIFT = await _settingsService.GetSettingAsync("SWIFT"),
            RoutingNumber = await _settingsService.GetSettingAsync("RoutingNumber"),
            BranchAddress = await _settingsService.GetSettingAsync("BranchAddress"),
            Currency = await _settingsService.GetSettingAsync("BankCurrency") ?? "USD"
        };

        return bankDetails;
    }

    public async Task<bool> VerifyPayPalPaymentAsync(string orderId, string paymentId)
    {
        try
        {
            var clientId = await _settingsService.GetSettingAsync("PayPalClientId");
            var clientSecret = await _settingsService.GetSettingAsync("PayPalClientSecret");
            var useSandbox = (await _settingsService.GetSettingAsync("PayPalUseSandbox"))?.ToLower() == "true";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("PayPal credentials not configured");
                return false;
            }

            var baseUrl = useSandbox
                ? "https://api.sandbox.paypal.com"
                : "https://api.paypal.com";

            var httpClient = _httpClientFactory.CreateClient();
            
            // Get access token
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            var tokenRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            tokenRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            var tokenResponse = await httpClient.SendAsync(tokenRequestMessage);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get PayPal access token");
                return false;
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            // Verify order
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var orderResponse = await httpClient.GetAsync($"{baseUrl}/v2/checkout/orders/{orderId}");
            
            if (!orderResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var orderContent = await orderResponse.Content.ReadAsStringAsync();
            var orderData = JsonSerializer.Deserialize<JsonElement>(orderContent);
            var status = orderData.GetProperty("status").GetString();

            return status?.ToUpper() == "COMPLETED";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PayPal payment {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> VerifyWisePaymentAsync(string transferId)
    {
        try
        {
            var apiKey = await _settingsService.GetSettingAsync("WiseApiKey");

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Wise API key not configured");
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.GetAsync($"https://api.transferwise.com/v1/transfers/{transferId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var transferData = JsonSerializer.Deserialize<JsonElement>(content);
            var status = transferData.GetProperty("status").GetString();

            return status?.ToUpper() == "COMPLETED" || status?.ToUpper() == "PAID_OUT";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Wise payment {TransferId}", transferId);
            return false;
        }
    }

    public async Task<PaymentResponseDto> UpdatePaymentStatusAsync(Guid paymentId, string status, string? transactionId = null)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found.");
        }

        if (Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
        {
            payment.Status = paymentStatus;
            if (paymentStatus == PaymentStatus.Completed)
            {
                payment.CompletedAt = DateTime.UtcNow;
                if (payment.Invoice.Status != InvoiceStatus.Paid)
                {
                    payment.Invoice.Status = InvoiceStatus.Paid;
                    payment.Invoice.PaidDate = DateTime.UtcNow;
                    payment.Invoice.PaymentMethod = payment.PaymentMethod;

                    // Notify Admin and SuperAdmin: Payment received
                    try
                    {
                        var firstOrderId = await _context.InvoiceOrders
                            .Where(io => io.InvoiceId == payment.InvoiceId && io.OrderId.HasValue)
                            .Select(io => io.OrderId)
                            .FirstOrDefaultAsync();
                        var title = "Payment Received";
                        var message = firstOrderId.HasValue
                            ? $"Payment received for order (#{NotificationFormatHelper.GetOrderNumber(firstOrderId.Value)})"
                            : $"Payment received for invoice (#{NotificationFormatHelper.GetInvoiceNumber(payment.Invoice.InvoiceNumber)})";
                        await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, payment.Invoice.Id, null);
                        await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, payment.Invoice.Id, null);
                    }
                    catch { /* Non-critical */ }
                }
            }
        }

        if (!string.IsNullOrEmpty(transactionId))
        {
            payment.TransactionId = transactionId;
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (payment.Status == PaymentStatus.Completed)
            InvalidateFinancialReadModels();

        return _mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<bool> VerifyPayPalWebhookSignatureAsync(string transmissionId, string transmissionTime, string transmissionSig, string authAlgo, string certUrl, string webhookEventJson)
    {
        try
        {
            var clientId = await _settingsService.GetSettingAsync("PayPalClientId");
            var clientSecret = await _settingsService.GetSettingAsync("PayPalClientSecret");
            var webhookId = await _settingsService.GetSettingAsync("PayPalWebhookId");
            var useSandbox = (await _settingsService.GetSettingAsync("PayPalUseSandbox"))?.ToLower() == "true";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("PayPal credentials not configured for webhook verification");
                return false;
            }

            if (string.IsNullOrEmpty(webhookId))
            {
                _logger.LogWarning("PayPalWebhookId not configured. Add webhook ID from PayPal Developer Dashboard.");
                return false;
            }

            var baseUrl = useSandbox
                ? "https://api.sandbox.paypal.com"
                : "https://api.paypal.com";

            var httpClient = _httpClientFactory.CreateClient();

            // Get access token
            var tokenRequest = new Dictionary<string, string> { { "grant_type", "client_credentials" } };
            var tokenRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            tokenRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            var tokenResponse = await httpClient.SendAsync(tokenRequestMessage);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get PayPal access token for webhook verification");
                return false;
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            // Call PayPal verify-webhook-signature API
            var verifyRequest = new
            {
                transmission_id = transmissionId,
                transmission_time = transmissionTime,
                transmission_sig = transmissionSig,
                auth_algo = authAlgo,
                cert_url = certUrl,
                webhook_id = webhookId,
                webhook_event = JsonSerializer.Deserialize<JsonElement>(webhookEventJson)
            };

            var verifyJson = JsonSerializer.Serialize(verifyRequest);
            var verifyRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/notifications/verify-webhook-signature")
            {
                Content = new StringContent(verifyJson, System.Text.Encoding.UTF8, "application/json")
            };
            verifyRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var verifyResponse = await httpClient.SendAsync(verifyRequestMessage);

            if (!verifyResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("PayPal webhook verification API returned {StatusCode}", verifyResponse.StatusCode);
                return false;
            }

            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var verifyData = JsonSerializer.Deserialize<JsonElement>(verifyContent);
            var status = verifyData.GetProperty("verification_status").GetString();

            return status?.ToUpper() == "SUCCESS";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PayPal webhook signature");
            return false;
        }
    }

    // Private helper methods
    private async Task<(string PaymentLink, string? TransactionId)> CreatePayPalPaymentAsync(CreatePaymentRequestDto request)
    {
        var clientId = await _settingsService.GetSettingAsync("PayPalClientId");
        var clientSecret = await _settingsService.GetSettingAsync("PayPalClientSecret");
        var useSandbox = (await _settingsService.GetSettingAsync("PayPalUseSandbox"))?.ToLower() == "true";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("PayPal credentials not configured. Please configure PayPal settings.");
        }

        var baseUrl = useSandbox
            ? "https://api.sandbox.paypal.com"
            : "https://api.paypal.com";

        var httpClient = _httpClientFactory.CreateClient();

        // Get access token
        var tokenRequest = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }
        };

            var tokenRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            tokenRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            var tokenResponse = await httpClient.SendAsync(tokenRequestMessage);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to authenticate with PayPal");
        }

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
        var accessToken = tokenData.GetProperty("access_token").GetString();

        // Create order
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

        var orderRequest = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = request.InvoiceId.ToString(),
                    amount = new
                    {
                        currency_code = request.Currency ?? "USD",
                        value = request.Amount.ToString("F2")
                    },
                    description = $"Invoice Payment - {request.InvoiceId}"
                }
            },
            application_context = new
            {
                return_url = request.ReturnUrl ?? $"{GetFrontendUrl()}/invoices?payment=success",
                cancel_url = request.CancelUrl ?? $"{GetFrontendUrl()}/invoices?payment=cancelled"
            }
        };

        var orderJson = JsonSerializer.Serialize(orderRequest);
        var orderResponse = await httpClient.PostAsync(
            $"{baseUrl}/v2/checkout/orders",
            new StringContent(orderJson, System.Text.Encoding.UTF8, "application/json"));

        if (!orderResponse.IsSuccessStatusCode)
        {
            var errorContent = await orderResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create PayPal order: {errorContent}");
        }

        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var orderData = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = orderData.GetProperty("id").GetString();
        
            // Get approval link
            var links = orderData.GetProperty("links").EnumerateArray();
            var approveLink = links.FirstOrDefault(l => 
                l.GetProperty("rel").GetString() == "approve");
            var approvalLink = approveLink.ValueKind != System.Text.Json.JsonValueKind.Undefined 
                ? approveLink.GetProperty("href").GetString() 
                : null;

        return (approvalLink ?? string.Empty, orderId);
    }

    private async Task<(string PaymentLink, string? TransactionId)> CreateWisePaymentAsync(CreatePaymentRequestDto request)
    {
        var apiKey = await _settingsService.GetSettingAsync("WiseApiKey");
        var profileId = await _settingsService.GetSettingAsync("WiseProfileId");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(profileId))
        {
            throw new InvalidOperationException("Wise credentials not configured. Please configure Wise settings.");
        }

        // Wise payment link generation
        // Note: This is a simplified implementation. In production, you'd use Wise's API to create quotes and transfers
        var paymentLink = $"{GetFrontendUrl()}/payments/wise/{request.InvoiceId}?amount={request.Amount}&currency={request.Currency ?? "USD"}";
        
        return (paymentLink, null);
    }

    private async Task<string> GenerateBankTransferLinkAsync(Guid invoiceId)
    {
        var frontendUrl = GetFrontendUrl();
        return $"{frontendUrl}/payments/bank/{invoiceId}";
    }

    private string GetFrontendUrl()
    {
        // This should be read from configuration
        return "http://localhost:4200"; // Default, should be configurable
    }
}
