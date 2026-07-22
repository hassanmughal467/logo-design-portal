namespace LogoDesignPortal.API.Extensions;

public static class HttpContextRateLimitExtensions
{
    /// <summary>
    /// Returns the caller's IP for rate-limiting keys. Deliberately does NOT read X-Forwarded-For
    /// directly - that header is attacker-controlled. ForwardedHeadersMiddleware (see Program.cs) already
    /// resolves Connection.RemoteIpAddress from a trusted proxy when applicable; for a direct/untrusted
    /// caller it is the real socket peer address, which cannot be spoofed by a request header.
    /// </summary>
    public static string GetRateLimitClientId(this HttpContext http)
    {
        return http.Connection.RemoteIpAddress != null
            ? http.Connection.RemoteIpAddress.ToString()
            : "unknown";
    }
}
