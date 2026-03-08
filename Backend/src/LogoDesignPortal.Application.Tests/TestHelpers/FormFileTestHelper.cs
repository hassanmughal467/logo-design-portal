using Microsoft.AspNetCore.Http;
using Moq;

namespace LogoDesignPortal.Application.Tests.TestHelpers;

/// <summary>
/// Helper for creating valid <see cref="IFormFile"/> instances for unit tests.
/// Uses Moq to avoid extra NuGet packages.
/// </summary>
public static class FormFileTestHelper
{
    /// <summary>
    /// Minimal PNG file header (8 bytes) - valid PNG magic bytes.
    /// </summary>
    private static readonly byte[] DefaultPngContent =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
    ];

    /// <summary>
    /// Creates a valid <see cref="IFormFile"/> for testing with PNG-like content.
    /// </summary>
    /// <param name="fileName">File name (default: "test.png").</param>
    /// <param name="contentType">Content type (default: "image/png").</param>
    /// <param name="content">Optional content bytes. If null, uses minimal PNG header bytes.</param>
    /// <returns>A mocked <see cref="IFormFile"/> with Length &gt; 0, FileName, ContentType, CopyToAsync, and OpenReadStream configured.</returns>
    public static IFormFile Create(
        string fileName = "test.png",
        string contentType = "image/png",
        byte[]? content = null)
    {
        var bytes = content ?? DefaultPngContent;
        if (bytes.Length == 0)
            bytes = DefaultPngContent;

        var mock = new Mock<IFormFile>();

        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(bytes.Length);
        mock.Setup(f => f.Name).Returns("file");

        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>(async (target, ct) =>
            {
                await target.WriteAsync(bytes, ct);
            });

        mock.Setup(f => f.OpenReadStream())
            .Returns(() => new MemoryStream(bytes));

        return mock.Object;
    }
}
