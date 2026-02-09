namespace LogoDesignPortal.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string userName);
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}
