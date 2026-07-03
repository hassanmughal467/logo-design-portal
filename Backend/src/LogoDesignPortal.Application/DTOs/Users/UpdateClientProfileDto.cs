using System.ComponentModel.DataAnnotations;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.DTOs.Users;

public class UpdateClientProfileDto
{
    [EmailAddress]
    public string? InvoiceEmail { get; set; }

    public BillingType? BillingType { get; set; }
    public string? CurrencyCode { get; set; }
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
    public CustomerType? CustomerType { get; set; }
}
