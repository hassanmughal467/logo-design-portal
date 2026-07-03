using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LogoDesignPortal.Infrastructure.Authentication;

/// <summary>
/// Builds signing keys and validation parameters for JWT key rotation.
/// Token issuance always uses <c>Jwt:Key</c> only; validation accepts the current and optional previous key.
/// </summary>
public static class JwtSigningKeyRotationHelper
{
    /// <summary>KeyId assigned to <c>Jwt:PreviousKey</c> so validated tokens can be traced to the rotation window.</summary>
    public const string PreviousKeyId = "previous";

    /// <summary>
    /// Returns signing keys for token validation: current <c>Jwt:Key</c> plus optional <c>Jwt:PreviousKey</c>.
    /// In production, both values are supplied via environment variables (<c>Jwt__Key</c>, <c>Jwt__PreviousKey</c>)
    /// through the standard ASP.NET Core configuration hierarchy — never commit real keys to appsettings.
    /// </summary>
    public static IList<SecurityKey> CreateValidationSigningKeys(IConfiguration configuration)
    {
        var keyVersion = configuration["Jwt:KeyVersion"] ?? "v1";
        var currentKeyValue = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");

        var keys = new List<SecurityKey>
        {
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(currentKeyValue))
            {
                KeyId = keyVersion
            }
        };

        var previousKeyValue = configuration["Jwt:PreviousKey"];
        if (!string.IsNullOrWhiteSpace(previousKeyValue))
        {
            keys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(previousKeyValue))
            {
                KeyId = PreviousKeyId
            });
        }

        return keys;
    }

    /// <summary>
    /// Builds <see cref="TokenValidationParameters"/> that accept both current and previous signing keys.
    /// </summary>
    public static TokenValidationParameters CreateTokenValidationParameters(
        IConfiguration configuration,
        string issuer,
        string audience,
        bool validateLifetime,
        TimeSpan? clockSkew = null)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKeys = CreateValidationSigningKeys(configuration),
            ClockSkew = clockSkew ?? TimeSpan.Zero,
            RequireExpirationTime = validateLifetime,
            RequireSignedTokens = true
        };
    }

    public static bool WasSignedWithPreviousKey(SecurityToken? token) =>
        token?.SigningKey is SymmetricSecurityKey { KeyId: PreviousKeyId };

    public static void LogPreviousKeyValidationWarning(ILogger logger, IConfiguration configuration)
    {
        var keyVersion = configuration["Jwt:KeyVersion"] ?? "v1";
        logger.LogWarning(
            "JWT validated using Jwt:PreviousKey (rotation in progress). KeyVersion={KeyVersion}",
            keyVersion);
    }
}
