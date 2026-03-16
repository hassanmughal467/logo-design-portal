namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Client Churn Prediction analytics for Super Admin.
/// Calculates risk scores and retention metrics to detect clients likely to stop ordering.
/// </summary>
public interface IClientChurnAnalyticsService
{
    Task<ClientChurnRiskScoresDto> GetClientRiskScoresAsync();
    Task<ClientChurnAlertsDto> GetClientChurnAlertsAsync(int inactiveDaysThreshold = 30);
    Task<ClientRetentionStatsDto> GetClientRetentionStatsAsync(int months = 12);
}

#region DTOs

public class ClientChurnRiskScoresDto
{
    public List<ClientRiskScoreItemDto> Items { get; set; } = new();
}

public class ClientRiskScoreItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int RiskScore { get; set; } // 0-100
    public string RiskLevel { get; set; } = string.Empty; // Healthy, Warning, HighRisk, ChurnLikely
    public int DaysSinceLastOrder { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueContributionPercent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class ClientChurnAlertsDto
{
    public List<ClientChurnAlertItemDto> Items { get; set; } = new();
}

public class ClientChurnAlertItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int DaysSinceLastOrder { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
}

public class ClientRetentionStatsDto
{
    public int TotalClientsWithOrders { get; set; }
    public int HealthyCount { get; set; }
    public int WarningCount { get; set; }
    public int HighRiskCount { get; set; }
    public int ChurnLikelyCount { get; set; }
    public List<ChurnRetentionTrendItemDto> RetentionTrend { get; set; } = new();
}

public class ChurnRetentionTrendItemDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int ActiveClients { get; set; }
    public int ChurnedClients { get; set; }
    public decimal RetentionRate { get; set; }
}

#endregion
