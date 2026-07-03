namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Client Intelligence and Client Revenue Analytics for Super Admin.
/// Provides chart-ready aggregated data for client behavior, revenue trends, and alerts.
/// </summary>
public interface IClientAnalyticsService
{
    Task<ClientAnalyticsOverviewDto> GetOverviewAsync();
    Task<TopClientsDto> GetTopClientsAsync(int limit = 10);
    Task<ClientRevenueTrendDto> GetClientRevenueTrendAsync(int months = 6);
    Task<ClientMonthlyRevenueDto> GetClientMonthlyRevenueAsync(Guid clientId, int months = 12);
    Task<InactiveClientsDto> GetInactiveClientsAsync(int inactiveDaysThreshold = 30);
    Task<ClientLifetimeValueDto> GetClientLifetimeValueAsync();
    Task<ClientGrowthDto> GetClientGrowthMetricsAsync();
    Task<ClientActivityDto> GetClientActivityStatusAsync();
    Task<ClientAlertsDto> GetClientAlertsAsync();
    Task<List<ClientDropdownItemDto>> GetClientsForDropdownAsync();
}

#region DTOs

public class ClientAnalyticsOverviewDto
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int InactiveClients { get; set; }
    public decimal TopClientRevenue { get; set; }
    /// <summary>ISO 4217 code of the top client whose revenue is reported in <see cref="TopClientRevenue"/>.</summary>
    public string TopClientCurrencyCode { get; set; } = "USD";
}

public class TopClientsDto
{
    public List<TopClientItemDto> Items { get; set; } = new();
}

public class TopClientItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    /// <summary>ISO 4217 code for this client's revenue. Sourced from ClientProfile.CurrencyCode.</summary>
    public string CurrencyCode { get; set; } = "USD";
}

public class ClientRevenueTrendDto
{
    public List<ClientRevenueTrendItemDto> Items { get; set; } = new();
    /// <summary>Single ISO code when all completed orders in the window share one currency; USD otherwise.</summary>
    public string CurrencyCode { get; set; } = "USD";
    /// <summary>True when the trend window contains completed orders in more than one currency.</summary>
    public bool CurrencyMixed { get; set; }
}

public class ClientRevenueTrendItemDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class ClientMonthlyRevenueDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public List<ClientMonthlyRevenueItemDto> Items { get; set; } = new();
    /// <summary>ISO 4217 code for this client (drives chart axis formatting).</summary>
    public string CurrencyCode { get; set; } = "USD";
}

public class ClientMonthlyRevenueItemDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class InactiveClientsDto
{
    public List<InactiveClientItemDto> Items { get; set; } = new();
}

public class InactiveClientItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime? LastOrderDate { get; set; }
    public int DaysSinceLastOrder { get; set; }
}

public class ClientLifetimeValueDto
{
    public List<ClientAnalyticsLifetimeValueItemDto> Items { get; set; } = new();
}

public class ClientAnalyticsLifetimeValueItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal LifetimeRevenue { get; set; }
    public int TotalOrders { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class ClientGrowthDto
{
    public List<ClientGrowthItemDto> Items { get; set; } = new();
}

public class ClientGrowthItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal LastMonthRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal RevenueChangePercent { get; set; }
    public string GrowthStatus { get; set; } = string.Empty; // Increase, Decrease, Stable
    public string CurrencyCode { get; set; } = "USD";
}

public class ClientActivityDto
{
    public List<ClientActivityItemDto> Items { get; set; } = new();
}

public class ClientActivityItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ActivityStatus { get; set; } = string.Empty; // Active, LowActivity, Inactive
    public DateTime? LastOrderDate { get; set; }
    public int DaysSinceLastOrder { get; set; }
}

public class ClientAlertsDto
{
    public List<ClientAlertItemDto> Items { get; set; } = new();
}

public class ClientAlertItemDto
{
    public string AlertType { get; set; } = string.Empty; // Inactive, RevenueIncrease, Milestone
    public string Message { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime OccurredAt { get; set; }
    /// <summary>ISO 4217 currency for revenue-related alert messages (Milestone, RevenueIncrease).</summary>
    public string CurrencyCode { get; set; } = "USD";
}

public class ClientDropdownItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
}

#endregion
