namespace LogoDesignPortal.Application.DTOs.Payments;

public class BankDetailsResponseDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string? SWIFT { get; set; }
    public string? RoutingNumber { get; set; }
    public string? BranchAddress { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Reference { get; set; } // Invoice number or payment reference
}
