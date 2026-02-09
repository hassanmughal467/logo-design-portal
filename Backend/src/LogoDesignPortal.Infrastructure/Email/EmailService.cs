using LogoDesignPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace LogoDesignPortal.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string userName)
    {
        var subject = "Password Reset Request - Hawk Merchandising Web Portal";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Hawk Merchandising Web Portal</h1>
        </div>
        <div class='content'>
            <h2>Password Reset Request</h2>
            <p>Hello {userName},</p>
            <p>You have requested to reset your password. Click the button below to reset your password:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #0066cc;'>{resetLink}</p>
            <p><strong>This link will expire in 24 hours.</strong></p>
            <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Hawk Merchandising Web Portal. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "Hawk Merchandising Web Portal";
            var frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:4200";

            // If email is not configured, log and return false
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Email configuration is missing. Email will not be sent. Configure Email settings in appsettings.json. See EMAIL_SETUP_GUIDE.md for instructions.");
                return false;
            }

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUsername, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }
}
