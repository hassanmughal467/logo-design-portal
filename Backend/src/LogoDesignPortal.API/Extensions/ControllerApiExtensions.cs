using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Extensions;

/// <summary>Consistent client-facing error payloads: <c>{ message, correlationId }</c>.</summary>
public static class ControllerApiExtensions
{
    public static object StandardError(this ControllerBase controller, string message)
        => new { message, correlationId = controller.HttpContext.GetCorrelationId() };
}
