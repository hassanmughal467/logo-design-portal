using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LogoDesignPortal.Infrastructure.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string> GenerateTokenAsync(User user)
    {
        var roleName = user.Role?.Name ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiry = DateTime.UtcNow;
        if (int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes) && minutes > 0)
        {
            expiry = expiry.AddMinutes(minutes);
        }
        else if (int.TryParse(_configuration["Jwt:AccessTokenExpiryHours"], out var hours))
        {
            expiry = expiry.AddHours(hours);
        }
        else
        {
            expiry = expiry.AddMinutes(15);
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a JWT and extracts the user id. Accepts tokens signed with either
    /// <c>Jwt:Key</c> (current) or <c>Jwt:PreviousKey</c> (rotation window).
    /// </summary>
    /// <remarks>
    /// Zero-downtime key rotation:
    /// <list type="number">
    /// <item><description>Generate a new key, set it as <c>Jwt:Key</c>, move the old value to <c>Jwt:PreviousKey</c>, bump <c>Jwt:KeyVersion</c>, and deploy.</description></item>
    /// <item><description>After <c>Jwt:RefreshTokenExpiryHours</c> days (refresh token lifetime), clear <c>Jwt:PreviousKey</c> and deploy again so only the new key is accepted.</description></item>
    /// </list>
    /// In production, <c>Jwt:Key</c> and <c>Jwt:PreviousKey</c> must be supplied via environment variables only
    /// (<c>Jwt__Key</c>, <c>Jwt__PreviousKey</c>) through the ASP.NET Core configuration hierarchy.
    /// </remarks>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = JwtSigningKeyRotationHelper.CreateTokenValidationParameters(
                _configuration,
                _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured"),
                _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured"),
                validateLifetime: false);

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (JwtSigningKeyRotationHelper.WasSignedWithPreviousKey(validatedToken))
            {
                JwtSigningKeyRotationHelper.LogPreviousKeyValidationWarning(_logger, _configuration);
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
