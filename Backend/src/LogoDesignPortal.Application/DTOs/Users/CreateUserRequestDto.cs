using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Users;

public class CreateUserRequestDto
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
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Guid RoleId { get; set; }

    // Additional User fields (optional)
    [EmailAddress]
    public string? SecondaryEmail { get; set; }

    [EmailAddress]
    public string? InvoiceEmail { get; set; }

    // Client Profile fields (optional, used when RoleId is Client)
    public string? CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Cell { get; set; }
    public string? Fax { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Website { get; set; }
    public string? Reference { get; set; }

    // Designer Profile fields (optional, used when RoleId is Designer)
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool? IsAvailable { get; set; }
}
