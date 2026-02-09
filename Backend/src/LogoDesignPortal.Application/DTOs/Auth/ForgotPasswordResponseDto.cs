namespace LogoDesignPortal.Application.DTOs.Auth;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; } // Only for development/testing - remove in production
    public string? Email { get; set; } // Only for development/testing - remove in production
    public string? ResetLink { get; set; } // Only for development/testing - remove in production
}
