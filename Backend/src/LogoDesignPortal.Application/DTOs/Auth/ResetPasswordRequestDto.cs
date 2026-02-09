using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}
