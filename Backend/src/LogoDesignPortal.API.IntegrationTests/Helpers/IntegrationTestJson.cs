using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogoDesignPortal.API.IntegrationTests.Helpers;

public static class IntegrationTestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
