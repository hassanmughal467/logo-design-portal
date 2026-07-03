namespace LogoDesignPortal.Application.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";

    public R2StorageOptions R2 { get; set; } = new();

    public LocalStorageOptions Local { get; set; } = new();

    public bool IsR2 =>
        string.Equals(Provider, "R2", StringComparison.OrdinalIgnoreCase);
}

public sealed class R2StorageOptions
{
    public string AccountId { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = "hawk-files";

    /// <summary>Optional CDN domain for public URLs. Leave empty to serve only via authorized API.</summary>
    public string PublicDomain { get; set; } = string.Empty;
}

public sealed class LocalStorageOptions
{
    public string BasePath { get; set; } = "Files";
}
