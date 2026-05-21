using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.API.IntegrationTests.Support.Factories;

public static class TestLogoFileFactory
{
    public static LogoFile CreateHiddenPreview(Guid orderId, string physicalPath, Guid uploadedBy)
    {
        return new LogoFile
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FileName = Path.GetFileName(physicalPath),
            OriginalFileName = "preview.png",
            FilePath = physicalPath,
            ContentType = "image/png",
            FileSize = 8,
            FileType = FileType.Preview,
            IsVisibleToClient = false,
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static async Task<string> WriteTempPngAsync()
    {
        var dir = Path.Combine(Path.GetTempPath(), "LDP_TestFile_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "test.png");
        await File.WriteAllBytesAsync(path, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        return path;
    }
}
