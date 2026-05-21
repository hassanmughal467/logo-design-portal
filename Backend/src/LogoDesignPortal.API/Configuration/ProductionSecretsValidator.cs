namespace LogoDesignPortal.API.Configuration;

/// <summary>
/// Fail-fast checks for unsafe production configuration (secrets, Swagger, default JWT keys).
/// </summary>
public static class ProductionSecretsValidator
{
    private static readonly string[] KnownPlaceholderJwtKeys =
    [
        "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong!",
        "CHANGE_ME_USE_ENVIRONMENT_VARIABLE_JWT_KEY_AT_LEAST_32_CHARS",
        "CHANGE_ME_USE_USER_SECRETS_OR_ENV_JWT_KEY_MIN_32_CHARS",
    ];

    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            return;

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Jwt:Key must be set via environment variables or a secret manager in production.");

        if (jwtKey.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters in production.");

        if (KnownPlaceholderJwtKeys.Any(p => string.Equals(jwtKey, p, StringComparison.Ordinal)))
            throw new InvalidOperationException(
                "Jwt:Key is still a template placeholder. Set Jwt__Key from environment variables or Azure Key Vault before production deployment.");

        if (!environment.IsDevelopment()
            && !environment.IsEnvironment("Testing")
            && configuration.GetValue("IncludeExceptionDetailsInProduction", false))
        {
            throw new InvalidOperationException(
                "IncludeExceptionDetailsInProduction must be false in Staging and Production to avoid leaking stack traces.");
        }
    }
}
