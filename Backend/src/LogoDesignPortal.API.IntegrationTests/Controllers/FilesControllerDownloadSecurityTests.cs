using System.Net;
using System.Net.Http.Headers;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class FilesControllerDownloadSecurityTests
{
    private readonly TestWebApplicationFactory _factory;

    public FilesControllerDownloadSecurityTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Download_HiddenPreview_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var fileId = await SeedHiddenPreviewFileAsync();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/files/{fileId}/download");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedHiddenPreviewFileAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = TestDataIds.ClientProfileId,
            Status = OrderStatus.InProgress,
            AllowUploads = true,
            CreatedAt = DateTime.UtcNow
        };
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        var storageDir = Path.Combine(Path.GetTempPath(), "LDP_IntegFile_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(storageDir);
        var path = Path.Combine(storageDir, "preview.png");
        await File.WriteAllBytesAsync(path, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        var file = new LogoFile
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FileName = "preview.png",
            OriginalFileName = "preview.png",
            FilePath = path,
            ContentType = "image/png",
            FileSize = 8,
            FileType = FileType.Preview,
            IsVisibleToClient = false,
            UploadedBy = TestDataIds.DesignerUserId,
            CreatedAt = DateTime.UtcNow
        };
        context.LogoFiles.Add(file);
        await context.SaveChangesAsync();
        return file.Id;
    }
}
