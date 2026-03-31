namespace LogoDesignPortal.Application.DTOs.Users;

/// <summary>Typeahead row for searchable user pickers (small payload).</summary>
public class UserTypeaheadDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    /// <summary>Display text for dropdowns, e.g. "Name (email)".</summary>
    public string Label { get; set; } = string.Empty;
}
