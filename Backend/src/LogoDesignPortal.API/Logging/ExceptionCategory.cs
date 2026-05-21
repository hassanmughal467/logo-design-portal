namespace LogoDesignPortal.API.Logging;

/// <summary>Stable categories for alerting and dashboards (Sentry/Grafana/Loki).</summary>
public enum ExceptionCategory
{
    Unknown,
    Authorization,
    Validation,
    NotFound,
    Conflict,
    Storage,
    ExternalDependency
}

public static class ExceptionCategoryMapper
{
    public static ExceptionCategory Map(Exception exception) => exception switch
    {
        LogoDesignPortal.Application.Exceptions.ForbiddenAccessException => ExceptionCategory.Authorization,
        UnauthorizedAccessException ua when ua.Message.Contains("Access to the path", StringComparison.OrdinalIgnoreCase)
            || ua.Message.Contains("is denied", StringComparison.OrdinalIgnoreCase) => ExceptionCategory.Storage,
        UnauthorizedAccessException => ExceptionCategory.Authorization,
        ArgumentException => ExceptionCategory.Validation,
        InvalidOperationException => ExceptionCategory.Validation,
        KeyNotFoundException => ExceptionCategory.NotFound,
        FileNotFoundException => ExceptionCategory.NotFound,
        Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => ExceptionCategory.Conflict,
        _ => ExceptionCategory.Unknown
    };
}
