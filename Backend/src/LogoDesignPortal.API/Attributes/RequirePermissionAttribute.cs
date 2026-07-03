using LogoDesignPortal.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LogoDesignPortal.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionName;

    public RequirePermissionAttribute(string permissionName)
    {
        _permissionName = permissionName;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
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
            context.Result = PermissionDeniedResult("Permission service is not configured.");
            return;
        }

        var userId = user.GetUserId();
        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = await permissionService.UserHasPermissionAsync(userId.Value, _permissionName);

        if (!hasPermission)
        {
            context.Result = PermissionDeniedResult(
                $"You do not have permission to perform this action ({_permissionName}). Please contact an administrator.");
        }
    }

    private static ObjectResult PermissionDeniedResult(string message) =>
        new(new { error = message }) { StatusCode = StatusCodes.Status403Forbidden };
}
