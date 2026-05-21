namespace LogoDesignPortal.Application.Constants;

/// <summary>Max multipart request / combined upload size (must match API RequestSizeLimit and IIS limits).</summary>
public static class UploadLimits
{
    /// <summary>Per-file maximum (design assets can be large; still bounded).</summary>
    public const long MaxSingleFileBytes = 100L * 1024 * 1024;

    /// <summary>Combined multipart cap for a single request.</summary>
    public const long MaxMultipartBytes = 500L * 1024 * 1024;
}
