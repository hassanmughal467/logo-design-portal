using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IClientProfileEnsureService, ClientProfileEnsureService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileUploadScanHook, NullFileUploadScanHook>();
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
        services.AddScoped<IQuoteService, QuoteService>();
        
        services.AddHttpClient(Options.DefaultName, client => client.Timeout = TimeSpan.FromSeconds(120))
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
                // Options validation: sampling duration must be >= 2 × attempt timeout (default was 30s each → startup crash).
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            });

        return services;
    }
}
