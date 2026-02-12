using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IRevisionService, RevisionService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
