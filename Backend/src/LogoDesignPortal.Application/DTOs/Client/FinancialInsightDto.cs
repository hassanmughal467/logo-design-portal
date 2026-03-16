namespace LogoDesignPortal.Application.DTOs.Client;

/// <summary>
/// A single financial insight message for the client dashboard.
/// </summary>
public class FinancialInsightDto
{
    public string Message { get; set; } = string.Empty;
    /// <summary>Optional: info, success, warning, danger</summary>
    public string Severity { get; set; } = "info";
    public string? Icon { get; set; }
}

/// <summary>
/// Response for GET /api/client/financial-insights
/// </summary>
public class FinancialInsightsResponseDto
{
    public List<FinancialInsightDto> Insights { get; set; } = new();
}
