namespace LogoDesignPortal.API.Extensions;

public static class HttpContextRateLimitExtensions
{
    public static string GetRateLimitClientId(this HttpContext http)
    {
        var fwd = http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(fwd))
        {
            var first = fwd.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                return first;
            }
        }

        return http.Connection.RemoteIpAddress != null
            ? http.Connection.RemoteIpAddress.ToString()
            : "unknown";
    }
}
