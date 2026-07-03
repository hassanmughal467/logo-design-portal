using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Invoices;

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    // Backward compatibility: first order ID
    public Guid OrderId { get; set; }

    // New: all order IDs
    public List<Guid> OrderIds { get; set; } = new List<Guid>();

    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public BillingType BillingType { get; set; }
    public string BillingTypeDisplay { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = string.Empty; // Paid, Unpaid, Overdue
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? BillingPeriod { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsLocked { get; set; } // True if status is Paid

    /// <summary>ISO currency from linked orders (e.g. GBP). Null if no order lines; PDF/UI fall back to invoice settings.</summary>
    public string? CurrencyCode { get; set; }

    /// <summary>USD→PKR rate locked at invoice issue time. Null when invoice has no PKR amounts.</summary>
    public decimal? ExchangeRate { get; set; }

    public DateTime? ExchangeRateFetchedAt { get; set; }

    public bool ExchangeRateIsStale { get; set; }
}
