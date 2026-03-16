namespace LogoDesignPortal.Application.Configuration;

/// <summary>
/// Configuration-based feature flags for production safety kill-switch.
/// Allows admins to disable modules instantly if a bug appears in production.
/// </summary>
public class ProductionSafetyOptions
{
    public const string SectionName = "ProductionSafety";

    /// <summary>When true, billing/invoice generation is disabled.</summary>
    public bool DisableBillingGeneration { get; set; }

    /// <summary>When true, designer payout generation is disabled.</summary>
    public bool DisableDesignerPayout { get; set; }

    /// <summary>When true, file uploads are disabled.</summary>
    public bool DisableFileUploads { get; set; }

    /// <summary>When true, invoice editing (update) is disabled.</summary>
    public bool DisableInvoiceEditing { get; set; }
}
