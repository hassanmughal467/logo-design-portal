using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Auth;

public class ResetPasswordWithTokenRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reset token is required.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
