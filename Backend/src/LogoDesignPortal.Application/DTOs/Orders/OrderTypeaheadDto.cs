namespace LogoDesignPortal.Application.DTOs.Orders;

/// <summary>Typeahead row for order pickers (small payload).</summary>
public class OrderTypeaheadDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    /// <summary>Human-readable line including pseudo order number where applicable.</summary>
    public string Label { get; set; } = string.Empty;
}
