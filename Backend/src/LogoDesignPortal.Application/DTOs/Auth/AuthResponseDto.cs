namespace LogoDesignPortal.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
    /// <summary>CSRF double-submit token for cookie auth (SPA stores when cookies are on the API host).</summary>
    public string? CsrfToken { get; set; }
}
