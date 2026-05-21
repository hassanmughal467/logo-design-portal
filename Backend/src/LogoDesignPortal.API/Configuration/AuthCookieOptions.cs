namespace LogoDesignPortal.API.Configuration;

public class AuthCookieOptions
{
    public const string SectionName = "AuthCookies";

    /// <summary>When true, login/refresh set httpOnly cookies and API accepts cookie auth.</summary>
    public bool Enabled { get; set; } = true;

    public string AccessTokenCookieName { get; set; } = "ldp_access";
    public string RefreshTokenCookieName { get; set; } = "ldp_refresh";
    public string CsrfCookieName { get; set; } = "ldp_csrf";
    public string CsrfHeaderName { get; set; } = "X-XSRF-TOKEN";

    /// <summary>Cookie path for auth cookies.</summary>
    public string Path { get; set; } = "/";

    public bool Secure { get; set; } = true;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.None;
}
