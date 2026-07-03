using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Infrastructure.Authentication;
using LogoDesignPortal.Infrastructure.Email;
using LogoDesignPortal.Infrastructure.Persistence;
using LogoDesignPortal.Infrastructure.Persistence.Repositories;
using LogoDesignPortal.Infrastructure.Currency;
using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace LogoDesignPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<SlowQueryLoggingInterceptor>();

        // Database - Use SQLite for local dev (no MySQL needed), MySQL for production
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<SlowQueryLoggingInterceptor>());

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            var migrationsAssembly = "LogoDesignPortal.Infrastructure";

            if (connectionString.Contains("Data Source=") || connectionString.Contains(".db"))
            {
                // SQLite - zero setup, works out of the box for local development
                options.UseSqlite(connectionString, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.CommandTimeout(120);
                });
            }
            else
            {
                // MySQL
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
                options.UseMySql(connectionString, serverVersion, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.CommandTimeout(120);
                    b.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });
            }
        });

        // Register DbContext as interface
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Authentication
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Email
        services.AddScoped<IEmailService, EmailService>();

        services.AddFileStorage(configuration);
        services.AddCurrencyService(configuration);

        return services;
    }
}
