using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Infrastructure.Authentication;
using LogoDesignPortal.Infrastructure.Email;
using LogoDesignPortal.Infrastructure.Persistence;
using LogoDesignPortal.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("LogoDesignPortal.Infrastructure")));

        // Register DbContext as interface
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Authentication
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Email
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
