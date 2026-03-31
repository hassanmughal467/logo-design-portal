using LogoDesignPortal.API.Middleware;

namespace LogoDesignPortal.API.Extensions;

public static class HttpContextCorrelationExtensions
{
    public static string GetCorrelationId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value) && value is string s && !string.IsNullOrEmpty(s))
            return s;
        return Guid.NewGuid().ToString("N")[..16];
    }
}
