namespace LogoDesignPortal.API.Middleware;

/// <summary>
/// Adds baseline security headers for browser clients. CSP is report-only by default; tighten via configuration in production.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        if (!_environment.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        // Report-only CSP — adjust connect-src/img-src for your frontend CDN and API host.
        if (!headers.ContainsKey("Content-Security-Policy-Report-Only"))
        {
            headers["Content-Security-Policy-Report-Only"] =
                "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
        }

        await _next(context);
    }
}
