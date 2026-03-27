using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;

namespace LogoDesignPortal.API.IntegrationTests.Support.Factories;

/// <summary>Builds <see cref="User"/> graphs for integration tests; override any property via <paramref name="configure"/>.</summary>
public static class TestUserFactory
{
    public static User CreateClient(
        string email,
        string passwordPlain,
        Guid? roleId = null,
        Action<User>? configure = null)
    {
        var role = roleId ?? SeededRoleIds.Client;
        var hash = BCrypt.Net.BCrypt.HashPassword(passwordPlain, BCrypt.Net.BCrypt.GenerateSalt(10));
        var u = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = hash,
            RoleId = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        configure?.Invoke(u);
        return u;
    }

    public static User CreateDesigner(string email, string passwordPlain, Action<User>? configure = null) =>
        CreateClient(email, passwordPlain, SeededRoleIds.Designer, configure);

    public static User CreateAdmin(string email, string passwordPlain, Action<User>? configure = null) =>
        CreateClient(email, passwordPlain, SeededRoleIds.Admin, configure);
}
