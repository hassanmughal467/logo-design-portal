namespace LogoDesignPortal.Application.Configuration;

/// <summary>Optional paths for invoice PDF branding. Embedded logo is used when <see cref="LogoPath"/> is unset or invalid.</summary>
public class InvoiceBrandingOptions
{
    public const string SectionName = "InvoiceBranding";

    /// <summary>Absolute path to a PNG or JPEG file for the invoice header logo.</summary>
    public string? LogoPath { get; set; }
}
