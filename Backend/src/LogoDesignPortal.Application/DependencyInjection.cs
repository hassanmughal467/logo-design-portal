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
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IRevisionService, RevisionService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
        services.AddScoped<IClientAnalyticsService, ClientAnalyticsService>();
        services.AddScoped<IClientChurnAnalyticsService, ClientChurnAnalyticsService>();
        services.AddScoped<IClientFinancialInsightsService, ClientFinancialInsightsService>();
        services.AddScoped<IDesignerPayoutService, DesignerPayoutService>();
        services.AddScoped<IClientLogoPricingService, ClientLogoPricingService>();
        services.AddScoped<IDesignerLogoPricingService, DesignerLogoPricingService>();
        
        // Add HttpClient for PaymentService
        services.AddHttpClient();

        return services;
    }
}
