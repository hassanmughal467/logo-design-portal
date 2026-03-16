namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Financial analytics service for admin dashboard - aggregated financial data.
/// Does not return raw order lists. Returns chart-ready grouped analytics.
/// </summary>
public interface IFinancialAnalyticsService
{
    Task<FinancialOverviewDto> GetOverviewAsync();
    Task<RevenueTrendDto> GetRevenueTrendAsync();
    Task<OrdersVsRevenueDto> GetOrdersVsRevenueAsync();
    Task<PackageRevenueDto> GetPackageRevenueAsync();
    Task<DesignerRevenueDto> GetDesignerRevenueAsync();
    Task<ClientRevenueDto> GetClientRevenueAsync();
    Task<InvoiceStatusDto> GetInvoiceStatusAsync();
    Task<WeeklyRevenueDto> GetWeeklyRevenueAsync();
    Task<OrderValueTrendDto> GetOrderValueTrendAsync();
    Task<FinancialActivityFeedDto> GetActivityFeedAsync();
    Task<RevenueForecastDto> GetRevenueForecastAsync();
}

public class FinancialOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
    public decimal RevenueGrowthPercent { get; set; }
    public int InvoicesPaid { get; set; }
    public int InvoicesPending { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
}

public class RevenueTrendDto
{
    public List<FinancialRevenueTrendItemDto> Items { get; set; } = new();
}

public class FinancialRevenueTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class OrdersVsRevenueDto
{
    public List<OrdersVsRevenueItemDto> Items { get; set; } = new();
}

public class OrdersVsRevenueItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
}

public class PackageRevenueDto
{
    public List<PackageRevenueItemDto> Items { get; set; } = new();
}

public class PackageRevenueItemDto
{
    public string Package { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class DesignerRevenueDto
{
    public List<DesignerRevenueItemDto> Items { get; set; } = new();
}

public class DesignerRevenueItemDto
{
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrdersCompleted { get; set; }
}

public class ClientRevenueDto
{
    public List<ClientRevenueItemDto> Items { get; set; } = new();
}

public class ClientRevenueItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class InvoiceStatusDto
{
    public List<InvoiceStatusItemDto> Items { get; set; } = new();
}

public class InvoiceStatusItemDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WeeklyRevenueDto
{
    public List<WeeklyRevenueItemDto> Items { get; set; } = new();
}

public class WeeklyRevenueItemDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int DayIndex { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderValueTrendDto
{
    public List<OrderValueTrendItemDto> Items { get; set; } = new();
}

public class OrderValueTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public decimal AverageOrderValue { get; set; }
}

public class FinancialActivityFeedDto
{
    public List<FinancialActivityItemDto> Items { get; set; } = new();
}

public class FinancialActivityItemDto
{
    public string Type { get; set; } = string.Empty; // InvoicePaid, NewOrder, OrderCompleted, RefundIssued, LargeTransaction
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class RevenueForecastDto
{
    public List<FinancialRevenueForecastItemDto> Items { get; set; } = new();
}

public class FinancialRevenueForecastItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public decimal? Actual { get; set; }
    public decimal? Predicted { get; set; }
}
