namespace LogoDesignPortal.Application.DTOs.Auth;

/// <summary>
/// Refresh credentials from JSON body and/or auth cookies (cookie-based SPA may send an empty body).
/// </summary>
public class RefreshTokenRequestDto
{
    public string? Token { get; set; }

    public string? RefreshToken { get; set; }
}
