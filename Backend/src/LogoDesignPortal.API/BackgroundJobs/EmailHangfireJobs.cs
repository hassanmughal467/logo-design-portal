using Hangfire;
using LogoDesignPortal.Application.Interfaces;

namespace LogoDesignPortal.API.BackgroundJobs;

/// <summary>Sends mail outside the HTTP request; retries on transient failures.</summary>
public class EmailHangfireJobs
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailHangfireJobs(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 10, 60, 300 })]
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
    {
        using var scope = _scopeFactory.CreateScope();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
        await email.SendEmailAsync(to, subject, body, isHtml).ConfigureAwait(false);
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 10, 60, 300 })]
    public async Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEmailService>();
        await svc.SendPasswordResetEmailAsync(email, resetLink, userName).ConfigureAwait(false);
    }
}
