using Microsoft.AspNetCore.Http;
using Xunit;

namespace LogoDesignPortal.Application.Tests.TestHelpers;

public class FormFileTestHelperTests
{
    [Fact]
    public void Create_ReturnsValidIFormFile()
    {
        var file = FormFileTestHelper.Create();

        Assert.NotNull(file);
        Assert.Equal("test.png", file.FileName);
        Assert.Equal("image/png", file.ContentType);
        Assert.True(file.Length > 0);
    }

    [Fact]
    public async Task Create_CopyToAsync_WritesContentToTarget()
    {
        var content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = FormFileTestHelper.Create(content: content);

        await using var target = new MemoryStream();
        await file.CopyToAsync(target);

        Assert.Equal(content, target.ToArray());
    }

    [Fact]
    public void Create_OpenReadStream_ReturnsStreamWithContent()
    {
        var content = new byte[] { 0x01, 0x02, 0x03 };
        var file = FormFileTestHelper.Create(content: content);

        using var stream = file.OpenReadStream();
        var buffer = new byte[content.Length];
        var read = stream.Read(buffer, 0, buffer.Length);

        Assert.Equal(content.Length, read);
        Assert.Equal(content, buffer);
    }

    [Fact]
    public void Create_WithCustomFileName_ReturnsCorrectFileName()
    {
        var file = FormFileTestHelper.Create(fileName: "custom.jpg", contentType: "image/jpeg");

        Assert.Equal("custom.jpg", file.FileName);
        Assert.Equal("image/jpeg", file.ContentType);
    }
}
