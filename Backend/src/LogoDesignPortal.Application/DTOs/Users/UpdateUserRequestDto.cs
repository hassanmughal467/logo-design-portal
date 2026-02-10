using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Users;

public class UpdateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty; // Role name (e.g., "SuperAdmin", "Admin", "Designer", "Client")

    [Required]
    public bool IsActive { get; set; }
}
