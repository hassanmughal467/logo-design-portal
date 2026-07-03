using System.Security.Cryptography;
using LogoDesignPortal.API.Configuration;
using LogoDesignPortal.Application.DTOs.Auth;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Services;

public interface IAuthCookieService
{
    bool IsEnabled { get; }
    /// <summary>Sets auth cookies; returns CSRF token when cookie auth is enabled.</summary>
    string? SetAuthCookies(HttpResponse response, AuthResponseDto auth);
    void ClearAuthCookies(HttpResponse response);
    string? GetAccessTokenFromRequest(HttpRequest request);
}

public sealed class AuthCookieService : IAuthCookieService
{
    private readonly AuthCookieOptions _options;
    private readonly IWebHostEnvironment _environment;

    public AuthCookieService(IOptions<AuthCookieOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public bool IsEnabled => _options.Enabled;

    public string? SetAuthCookies(HttpResponse response, AuthResponseDto auth)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var secure = _options.Secure;
        var sameSite = _options.SameSite;

        response.Cookies.Append(_options.AccessTokenCookieName, auth.Token,
            BuildCookieOptions(new DateTimeOffset(auth.ExpiresAt, TimeSpan.Zero), httpOnly: true, secure, sameSite, _options.Path));
        response.Cookies.Append(_options.RefreshTokenCookieName, auth.RefreshToken,
            BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(7), httpOnly: true, secure, sameSite, _options.Path));

        var csrf = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);
        response.Cookies.Append(_options.CsrfCookieName, csrf,
            BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(7), httpOnly: false, secure, sameSite, _options.Path));
        return csrf;
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var expired = DateTimeOffset.UtcNow.AddDays(-1);
        var opts = new CookieOptions { Path = _options.Path, Expires = expired, HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax };
        response.Cookies.Delete(_options.AccessTokenCookieName, opts);
        response.Cookies.Delete(_options.RefreshTokenCookieName, opts);
        response.Cookies.Delete(_options.CsrfCookieName, new CookieOptions { Path = _options.Path, Expires = expired });
    }

    public string? GetAccessTokenFromRequest(HttpRequest request)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        return request.Cookies.TryGetValue(_options.AccessTokenCookieName, out var token) ? token : null;
    }

    private static CookieOptions BuildCookieOptions(DateTimeOffset expires, bool httpOnly, bool secure, SameSiteMode sameSite, string path) =>
        new()
        {
            HttpOnly = httpOnly,
            Secure = secure,
            SameSite = sameSite,
            Path = path,
            Expires = expires,
            IsEssential = true
        };
}
