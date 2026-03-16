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
    /// Gets the user ID or throws UnauthorizedAccessException if not found. Use when [Authorize] guarantees authentication.
    /// Returns 401 when claim is missing.
    /// </summary>
    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
    {
        var id = user.GetUserId();
        if (id == null)
            throw new UnauthorizedAccessException("User identity could not be determined.");
        return id.Value;
    }
}
