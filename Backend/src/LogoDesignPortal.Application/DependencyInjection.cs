using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
