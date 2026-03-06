namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Helper for formatting display numbers used in notification messages.
/// </summary>
public static class NotificationFormatHelper
{
    /// <summary>
    /// Formats order ID as display number, e.g. ORD-A1B2C3D4.
    /// </summary>
    public static string GetOrderNumber(Guid orderId)
    {
        return $"ORD-{orderId.ToString("N")[..8].ToUpperInvariant()}";
    }

    /// <summary>
    /// Formats invoice number for display (pass through if already formatted).
    /// </summary>
    public static string GetInvoiceNumber(string? invoiceNumber)
    {
        return string.IsNullOrEmpty(invoiceNumber) ? "INV-Unknown" : invoiceNumber;
    }
}
