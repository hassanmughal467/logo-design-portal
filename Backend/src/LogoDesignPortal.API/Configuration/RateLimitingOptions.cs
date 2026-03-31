namespace LogoDesignPortal.API.Configuration;

/// <summary>Per-route-class limits (requests per rolling minute window).</summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int GeneralPerMinute { get; set; } = 60;
    public int AuthPerMinute { get; set; } = 5;
    public int OrdersMutationsPerMinute { get; set; } = 120;
    public int FileUploadPerMinute { get; set; } = 30;
}
