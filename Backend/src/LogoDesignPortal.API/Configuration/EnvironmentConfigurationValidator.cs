namespace LogoDesignPortal.API.Configuration;

/// <summary>
/// Fail-fast checks for environment-specific configuration (connection strings, Redis, storage paths).
/// Complements <see cref="ProductionSecretsValidator"/> for non-secret structural safety.
/// </summary>
public static class EnvironmentConfigurationValidator
{
    private static readonly string[] KnownConnectionStringPlaceholders =
    [
        "YOUR_MYSQL_PASSWORD",
        "YOUR_STAGING_PASSWORD",
        "REPLACE_IN_WEB_CONFIG",
        "REPLACE_IN_USER_SECRETS_OR_ENV",
        "REPLACE_IN_ENV",
        "REPLACE_WITH_ACTUAL",
    ];

    private static readonly Dictionary<string, string> ExpectedFileStoragePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Development"] = "Files_Dev",
        ["Testing"] = "Files_Test",
        ["Staging"] = "Files_Staging",
        ["Production"] = "Files",
    };

    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return;
        }

        ValidateConnectionString(configuration, environment);
        ValidateFileStoragePath(configuration, environment);
    }

    private static void ValidateConnectionString(IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string not configured for {environment.EnvironmentName}. " +
                "Set ConnectionStrings__DefaultConnection via environment variables, IIS web.config, or a secret manager.");
        }

        foreach (var placeholder in KnownConnectionStringPlaceholders)
        {
            if (connectionString.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Database connection string contains placeholder '{placeholder}' in {environment.EnvironmentName}. " +
                    "Set ConnectionStrings__DefaultConnection to a real credential before deployment.");
            }
        }
    }

    private static void ValidateFileStoragePath(IConfiguration configuration, IHostEnvironment environment)
    {
        var path = configuration["FileStorage:Path"];
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                $"FileStorage:Path must be set for {environment.EnvironmentName}.");
        }

        if (!ExpectedFileStoragePaths.TryGetValue(environment.EnvironmentName, out var expected))
        {
            return;
        }

        if (!string.Equals(path, expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"FileStorage:Path for {environment.EnvironmentName} should be '{expected}' to isolate uploads. " +
                $"Current value: '{path}'. Override only when using dedicated per-host volumes documented in ops runbooks.");
        }
    }
}
