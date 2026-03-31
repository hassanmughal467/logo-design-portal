namespace LogoDesignPortal.Application.BackgroundJobs;

/// <summary>No-op scheduler for hosts/tests that do not run Hangfire.</summary>
public sealed class NullBackgroundJobScheduler : IBackgroundJobScheduler
{
    public void EnqueueSendEmail(string to, string subject, string body, bool isHtml = true) { }

    public void EnqueuePasswordResetEmail(string email, string resetLink, string userName) { }
}
