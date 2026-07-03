using System.Security.Claims;

namespace LogoDesignPortal.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Safely gets the user ID from claims. Returns null if claim is missing or invalid.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Resolves the application role from JWT claims (handles <see cref="ClaimTypes.Role"/>, short "role", and duplicate entries).
    /// </summary>
    public static string? GetUserRole(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var values = user.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role
                || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value?.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (values.Count == 0)
        {
            return null;
        }

        // Prefer canonical names when the token carries multiple role-like claims
        foreach (var preferred in new[] { "SuperAdmin", "Admin", "Designer", "Client" })
        {
            if (values.Any(v => string.Equals(v, preferred, StringComparison.OrdinalIgnoreCase)))
            {
                return preferred;
            }
        }

        return values[0];
    }

    /// <summary>
    /// Gets the user ID or throws UnauthorizedAccessException if not found. Use when [Authorize] guarantees authentication.
    /// Returns 401 when claim is missing.
    /// </summary>
    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
    {
        var id = user.GetUserId();
        if (id == null)
        {
            throw new UnauthorizedAccessException("User identity could not be determined.");
        }

        return id.Value;
    }
}
