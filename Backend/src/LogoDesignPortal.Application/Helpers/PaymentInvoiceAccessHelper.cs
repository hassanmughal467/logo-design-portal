using LogoDesignPortal.Domain.Entities;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Access rules for client billing invoices used by payment mutations (create, link, process).
/// </summary>
public static class PaymentInvoiceAccessHelper
{
    public static bool CanAccessInvoice(Invoice invoice, Guid userId, string? userRole)
    {
        if (userRole is "Admin" or "SuperAdmin")
            return true;

        if (userRole == "Client")
            return invoice.Client != null && invoice.Client.UserId == userId;

        return false;
    }
}
