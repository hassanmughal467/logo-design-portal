namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Machine-readable keys for invoice update notifications (embedded in notification message).
/// </summary>
public static class InvoiceUpdateChangeLabels
{
    public const string NewLogoAdded = "new_logo_added";
    public const string PriceUpdated = "price_updated";
    public const string DueDateExtended = "due_date_extended";
    public const string DueDateUpdated = "due_date_updated";
    public const string LineItemRemoved = "line_item_removed";

    public static string ToDisplayText(string key) => key switch
    {
        NewLogoAdded => "new logo added",
        PriceUpdated => "price updated",
        DueDateExtended => "due date extended",
        DueDateUpdated => "due date updated",
        LineItemRemoved => "line item removed",
        _ => key.Replace('_', ' ')
    };

    public static string FormatNotificationMessage(string invoiceDisplayNumber, IEnumerable<string> changeKeys)
    {
        var labels = changeKeys
            .Distinct(StringComparer.Ordinal)
            .Select(ToDisplayText)
            .ToList();
        var summary = labels.Count == 0
            ? "details updated"
            : string.Join(", ", labels);
        return $"Invoice #{invoiceDisplayNumber} was updated: {summary}.";
    }
}
