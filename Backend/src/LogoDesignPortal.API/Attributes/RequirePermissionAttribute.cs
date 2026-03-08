using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LogoDesignPortal.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permissionName;

    public RequirePermissionAttribute(string permissionName)
    {
        _permissionName = permissionName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // SuperAdmin always has all permissions (bypass check)
        var userRole = user.FindFirstValue(ClaimTypes.Role);
        if (userRole == "SuperAdmin")
        {
            return; // SuperAdmin has all permissions automatically
        }

        // Get permission service
        var permissionService = context.HttpContext.RequestServices
            .GetService<LogoDesignPortal.Application.Interfaces.IPermissionService>();

        if (permissionService == null)
        {
            // Deny access when permission service is not configured to prevent bypass in misconfigured environments
            context.Result = new ForbidResult();
            return;
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check permission asynchronously
        var hasPermission = permissionService.UserHasPermissionAsync(userId, _permissionName).GetAwaiter().GetResult();

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
