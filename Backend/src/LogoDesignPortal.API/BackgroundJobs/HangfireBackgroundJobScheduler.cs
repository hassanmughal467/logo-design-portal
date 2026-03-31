using Hangfire;
using LogoDesignPortal.Application.BackgroundJobs;

namespace LogoDesignPortal.API.BackgroundJobs;

public sealed class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    public void EnqueueSendEmail(string to, string subject, string body, bool isHtml = true)
    {
        BackgroundJob.Enqueue<EmailHangfireJobs>(j => j.SendEmailAsync(to, subject, body, isHtml));
    }

    public void EnqueuePasswordResetEmail(string email, string resetLink, string userName)
    {
        BackgroundJob.Enqueue<EmailHangfireJobs>(j => j.SendPasswordResetEmailAsync(email, resetLink, userName));
    }
}
