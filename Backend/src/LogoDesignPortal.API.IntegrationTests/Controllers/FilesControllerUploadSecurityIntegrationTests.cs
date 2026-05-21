using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class FilesControllerUploadSecurityIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public FilesControllerUploadSecurityIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    private async Task<Guid> CreateOrderWithUploadsEnabledAsync(HttpClient client)
    {
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Upload test", description = "d", price = 25 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdDto>(IntegrationTestJson.Options);
        return order!.Id;
    }

    [Fact]
    public async Task Upload_DisallowedExtension_Returns400()
    {
        var client = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(client);

        var exeBytes = new byte[] { 0x4D, 0x5A };
        var response = await MultipartTestHelper.PostFileUploadAsync(
            client, orderId, "malware.exe", exeBytes, "application/octet-stream");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_PngExtensionWithPdfMagicBytes_Returns400()
    {
        var client = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(client);

        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var response = await MultipartTestHelper.PostFileUploadAsync(
            client, orderId, "fake.png", pdfHeader, "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ValidPng_Returns200()
    {
        var client = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(client);

        var response = await MultipartTestHelper.PostFileUploadAsync(
            client, orderId, "ref.png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            "image/png");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutAuth_Returns401()
    {
        var authed = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(authed);

        var anon = _factory.CreateClient();
        var response = await MultipartTestHelper.PostFileUploadAsync(
            anon, orderId, "ref.png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            "image/png");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_DoubleExtension_Returns400()
    {
        var client = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(client);

        var response = await MultipartTestHelper.PostFileUploadAsync(
            client, orderId, "logo.png.exe",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_AsDesignerToUnassignedOrder_Returns403()
    {
        var clientOrder = _factory.CreateClient();
        var orderId = await CreateOrderWithUploadsEnabledAsync(clientOrder);

        var designer = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(designer);
        designer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", designerToken);

        var response = await MultipartTestHelper.PostFileUploadAsync(
            designer, orderId, "preview.png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            "image/png",
            fileType: FileType.Preview.ToString());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed class OrderIdDto
    {
        public Guid Id { get; set; }
    }
}
