namespace LogoDesignPortal.Application.BackgroundJobs;

/// <summary>
/// Enqueues heavy work (email, invoicing follow-ups) so API handlers return quickly.
/// Implemented with Hangfire in the host; use no-op in isolated tests if needed.
/// </summary>
public interface IBackgroundJobScheduler
{
    void EnqueueSendEmail(string to, string subject, string body, bool isHtml = true);

    void EnqueuePasswordResetEmail(string email, string resetLink, string userName);
}
