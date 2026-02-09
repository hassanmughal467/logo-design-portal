using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LogoDesignPortal.Application.DTOs.Auth;

public class ChangePasswordRequestDto
{
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
