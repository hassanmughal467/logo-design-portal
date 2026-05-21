using System.Net;
using System.Text;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class PaymentsControllerWebhookSecurityTests
{
    private readonly TestWebApplicationFactory _factory;

    public PaymentsControllerWebhookSecurityTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PayPalWebhook_EmptyBody_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/payments/webhook/paypal",
            new StringContent("", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PayPalWebhook_MissingVerificationHeaders_Returns401()
    {
        var client = _factory.CreateClient();
        var body = """{"event_type":"PAYMENT.CAPTURE.COMPLETED"}""";
        var response = await client.PostAsync("/api/payments/webhook/paypal",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
