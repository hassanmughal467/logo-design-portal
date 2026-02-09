using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;
}
