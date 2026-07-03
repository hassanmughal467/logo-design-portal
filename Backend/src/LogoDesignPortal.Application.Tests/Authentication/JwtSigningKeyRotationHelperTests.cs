using LogoDesignPortal.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Authentication;

public class JwtSigningKeyRotationHelperTests
{
    private const string CurrentKey = "current-signing-key-at-least-32-chars!!";
    private const string PreviousKey = "previous-signing-key-at-least-32-chars!";
    private const string Issuer = "LogoDesignPortal";
    private const string Audience = "LogoDesignPortalUsers";

    [Fact]
    public void CreateValidationSigningKeys_WithPreviousKey_ReturnsBothKeys()
    {
        var configuration = BuildConfiguration(CurrentKey, PreviousKey);

        var keys = JwtSigningKeyRotationHelper.CreateValidationSigningKeys(configuration);

        Assert.Equal(2, keys.Count);
        Assert.Equal("v1", keys[0].KeyId);
        Assert.Equal(JwtSigningKeyRotationHelper.PreviousKeyId, keys[1].KeyId);
    }

    [Fact]
    public void CreateValidationSigningKeys_WithoutPreviousKey_ReturnsCurrentKeyOnly()
    {
        var configuration = BuildConfiguration(CurrentKey, previousKey: null);

        var keys = JwtSigningKeyRotationHelper.CreateValidationSigningKeys(configuration);

        Assert.Single(keys);
        Assert.Equal("v1", keys[0].KeyId);
    }

    [Fact]
    public void CreateTokenValidationParameters_AcceptsTokenSignedWithPreviousKey()
    {
        var configuration = BuildConfiguration(CurrentKey, PreviousKey);
        var token = CreateSignedToken(PreviousKey, Guid.NewGuid());
        var handler = new JwtSecurityTokenHandler();
        var parameters = JwtSigningKeyRotationHelper.CreateTokenValidationParameters(
            configuration,
            Issuer,
            Audience,
            validateLifetime: false);

        var principal = handler.ValidateToken(token, parameters, out SecurityToken validatedToken);

        Assert.NotNull(principal.FindFirst(ClaimTypes.NameIdentifier));
        Assert.True(JwtSigningKeyRotationHelper.WasSignedWithPreviousKey(validatedToken));
    }

    [Fact]
    public void CreateTokenValidationParameters_AcceptsTokenSignedWithCurrentKey()
    {
        var configuration = BuildConfiguration(CurrentKey, PreviousKey);
        var token = CreateSignedToken(CurrentKey, Guid.NewGuid());
        var handler = new JwtSecurityTokenHandler();
        var parameters = JwtSigningKeyRotationHelper.CreateTokenValidationParameters(
            configuration,
            Issuer,
            Audience,
            validateLifetime: false);

        var principal = handler.ValidateToken(token, parameters, out SecurityToken validatedToken);

        Assert.NotNull(principal.FindFirst(ClaimTypes.NameIdentifier));
        Assert.False(JwtSigningKeyRotationHelper.WasSignedWithPreviousKey(validatedToken));
    }

    [Fact]
    public void GetUserIdFromToken_WithPreviousKey_LogsWarningAndReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var configuration = BuildConfiguration(CurrentKey, PreviousKey);
        var token = CreateSignedToken(PreviousKey, userId);
        var service = new JwtTokenService(configuration, NullLogger<JwtTokenService>.Instance);

        var result = service.GetUserIdFromToken(token);

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserIdFromToken_WithCurrentKeyOnlyConfigured_RejectsPreviousKeyToken()
    {
        var configuration = BuildConfiguration(CurrentKey, previousKey: null);
        var token = CreateSignedToken(PreviousKey, Guid.NewGuid());
        var service = new JwtTokenService(configuration, NullLogger<JwtTokenService>.Instance);

        var result = service.GetUserIdFromToken(token);

        Assert.Null(result);
    }

    private static IConfiguration BuildConfiguration(string currentKey, string? previousKey)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = currentKey,
            ["Jwt:KeyVersion"] = "v1",
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience
        };

        if (previousKey != null)
        {
            values["Jwt:PreviousKey"] = previousKey;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static string CreateSignedToken(string signingKey, Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
