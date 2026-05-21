namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Optional malware / content scan hook for uploads. Register a production implementation (e.g. ClamAV, cloud scanner).
/// </summary>
public interface IFileUploadScanHook
{
    Task ScanAsync(string storedFilePath, string originalFileName, CancellationToken cancellationToken = default);
}
