using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Infrastructure.Data;
using LogoDesignPortal.Infrastructure.Repositories;
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

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
