using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Constants;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class UsersControllerTests
{
    private readonly TestWebApplicationFactory _factory;

    public UsersControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllUsers_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_AsSuperAdmin_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMyPermissions_AsClient_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users/me/permissions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions);
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetUserById_OtherUser_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/users/{_factory.AdminUserId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_AsSuperAdmin_Returns201()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var email = $"created.integration.{Guid.NewGuid():N}@test.com";
        var payload = new
        {
            email,
            firstName = "Created",
            lastName = "User",
            password = "CreateUser@123",
            roleId = SeededRoleIds.Designer
        };

        var response = await client.PostAsJsonAsync("/api/users", payload, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            email = $"forbidden.create.{Guid.NewGuid():N}@test.com",
            firstName = "No",
            lastName = "Access",
            password = "CreateUser@123",
            roleId = SeededRoleIds.Designer
        };

        var response = await client.PostAsJsonAsync("/api/users", payload, JsonOptions);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            email = "client@test.com",
            firstName = "Duplicate",
            lastName = "User",
            password = "CreateUser@123",
            roleId = SeededRoleIds.Client
        };

        var response = await client.PostAsJsonAsync("/api/users", payload, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
