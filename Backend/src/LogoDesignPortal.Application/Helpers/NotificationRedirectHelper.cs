using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Builds frontend redirect URLs for notifications based on reference type and IDs.
/// </summary>
public static class NotificationRedirectHelper
{
    /// <summary>
    /// Builds the redirect URL for a notification. Returns null if no navigable target.
    /// </summary>
    public static string? BuildRedirectUrl(
        NotificationReferenceType referenceType,
        Guid? referenceId,
        Guid? orderId)
    {
        var refId = referenceId ?? orderId;
        if (!refId.HasValue)
        {
            return null;
        }

        return referenceType switch
        {
            NotificationReferenceType.Order => $"/orders/{refId}",
            NotificationReferenceType.Invoice => $"/invoices/{refId}",
            NotificationReferenceType.Message => orderId.HasValue ? $"/orders/{orderId}" : "/messages",
            NotificationReferenceType.System => "/users",
            NotificationReferenceType.Quote => $"/quotes/{refId}",
            _ => orderId.HasValue ? $"/orders/{orderId}" : null
        };
    }
}
