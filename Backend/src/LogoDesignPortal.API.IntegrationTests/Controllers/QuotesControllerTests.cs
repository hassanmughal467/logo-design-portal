using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class QuotesControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions = IntegrationTestJson.Options;
    private readonly TestWebApplicationFactory _factory;

    public QuotesControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateQuote_WithAttachment_ReturnsAttachmentsForSuperAdmin()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var quotePayload = new
        {
            logoName = "Quote With Files",
            description = "Integration test quote with PNG attachment",
            requestedBudget = 100m
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(quotePayload, JsonOptions)), "quote");

        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "files", "reference.png");

        var createResponse = await client.PostAsync("/api/quotes", form);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.True(createResponse.IsSuccessStatusCode,
            $"Expected success but got {(int)createResponse.StatusCode}. Body: {createBody}");

        var created = JsonSerializer.Deserialize<QuoteResponse>(createBody, JsonOptions);
        Assert.NotNull(created);
        Assert.NotEmpty(created!.Attachments);

        var adminClient = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getResponse = await adminClient.GetAsync($"/api/quotes/{created.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.IsSuccessStatusCode, getBody);

        var fetched = JsonSerializer.Deserialize<QuoteResponse>(getBody, JsonOptions);
        Assert.NotNull(fetched);
        Assert.NotEmpty(fetched!.Attachments);

        var storedName = fetched.Attachments[0];
        var downloadResponse = await adminClient.GetAsync($"/api/quotes/{created.Id}/attachments/{storedName}");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        Assert.True(downloadResponse.Content.Headers.ContentLength > 0);

        var designerClient = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(designerClient);
        designerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", designerToken);
        var forbidden = await designerClient.GetAsync($"/api/quotes/{created.Id}/attachments/{storedName}");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task CreateQuote_WithoutFiles_SucceedsWithEmptyAttachments()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var quotePayload = new
        {
            logoName = "Quote No Files",
            description = "No attachments in this request",
            requestedBudget = (decimal?)null
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(quotePayload, JsonOptions)), "quote");

        var response = await client.PostAsync("/api/quotes", form);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        var created = JsonSerializer.Deserialize<QuoteResponse>(body, JsonOptions);
        Assert.NotNull(created);
        Assert.Empty(created!.Attachments);
    }

    [Fact]
    public async Task RejectQuote_AsClient_AdminAndSuperAdminReceiveNotification()
    {
        var clientHttp = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(clientHttp);
        clientHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var quotePayload = new
        {
            logoName = "Reject Notify Test",
            description = "Client will decline after admin responds",
            requestedBudget = (decimal?)null
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(quotePayload, JsonOptions)), "quote");
        var createResponse = await clientHttp.PostAsync("/api/quotes", form);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<QuoteRejectResponse>(JsonOptions);
        Assert.NotNull(created);

        var adminHttp = _factory.CreateClient();
        var adminToken = await AuthHelper.GetAdminTokenAsync(adminHttp);
        adminHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var respondResponse = await adminHttp.PostAsJsonAsync(
            $"/api/quotes/{created!.Id}/respond",
            new { adminQuotedPrice = 4m, adminNotes = "Test price" },
            JsonOptions);
        respondResponse.EnsureSuccessStatusCode();

        var rejectResponse = await clientHttp.PostAsync($"/api/quotes/{created.Id}/reject", null);
        rejectResponse.EnsureSuccessStatusCode();

        var superAdminHttp = _factory.CreateClient();
        var superAdminToken = await AuthHelper.GetSuperAdminTokenAsync(superAdminHttp);
        superAdminHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", superAdminToken);

        var superAdminNotifications = await superAdminHttp.GetFromJsonAsync<List<NotificationItem>>("/api/notifications", JsonOptions);
        Assert.NotNull(superAdminNotifications);
        Assert.Contains(superAdminNotifications!, n =>
            n.Title == "Quote Declined"
            && (n.Message?.Contains("Reject Notify Test", StringComparison.OrdinalIgnoreCase) ?? false)
            && (n.ReferenceId == created.Id));

        var adminNotifications = await adminHttp.GetFromJsonAsync<List<NotificationItem>>("/api/notifications", JsonOptions);
        Assert.NotNull(adminNotifications);
        Assert.Contains(adminNotifications!, n =>
            n.Title == "Quote Declined"
            && (n.Message?.Contains("Reject Notify Test", StringComparison.OrdinalIgnoreCase) ?? false)
            && (n.ReferenceId == created.Id));
    }

    private sealed class QuoteResponse
    {
        public Guid Id { get; set; }
        public List<string> Attachments { get; set; } = new();
    }

    private sealed class QuoteRejectResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class NotificationItem
    {
        public Guid Id { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
