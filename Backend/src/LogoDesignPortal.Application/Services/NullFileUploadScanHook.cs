using LogoDesignPortal.Application.Interfaces;

namespace LogoDesignPortal.Application.Services;

/// <summary>No-op scan hook until a production scanner is wired.</summary>
public sealed class NullFileUploadScanHook : IFileUploadScanHook
{
    public Task ScanAsync(string storedFilePath, string originalFileName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
