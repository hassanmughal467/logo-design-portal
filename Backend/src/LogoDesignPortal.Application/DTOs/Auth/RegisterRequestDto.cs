using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [EmailAddress]
    public string? SecondaryEmail { get; set; }

    [Required]
    [EmailAddress]
    public string InvoiceEmail { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? Cell { get; set; }

    public string? Fax { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? ZipCode { get; set; }

    public string? State { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public string? Reference { get; set; }

    [Required]
    public bool AgreeToTerms { get; set; }
}
