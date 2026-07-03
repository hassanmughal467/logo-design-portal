namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Analytics service for Super Admin dashboard - aggregated data for charts and KPIs.
/// Does not break existing order lifecycle, notifications, or workflow logic.
/// </summary>
public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync();
    Task<OrderAnalyticsDto> GetOrderAnalyticsAsync();
    Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync();
    Task<DesignerAnalyticsDto> GetDesignerAnalyticsAsync();
    Task<ClientAnalyticsDto> GetClientAnalyticsAsync();
    Task<WorkflowAnalyticsDto> GetWorkflowAnalyticsAsync();
    Task<SystemAnalyticsDto> GetSystemAnalyticsAsync();
    Task<ForecastAnalyticsDto> GetForecastAnalyticsAsync();
    Task<InsightsAnalyticsDto> GetInsightsAsync();
}

public class AnalyticsOverviewDto
{
    public int TotalOrders { get; set; }
    public int OrdersToday { get; set; }
    public int OrdersThisMonth { get; set; }
    public int OrdersThisYear { get; set; }
    /// <summary>
    /// Cross-currency raw sum across all completed orders (NOT FX-converted).
    /// Only meaningful when <see cref="RevenueCurrencyMixed"/> is false. When mixed,
    /// the UI must use <see cref="RevenueByCurrency"/> for accurate per-currency totals.
    /// </summary>
    public decimal TotalRevenue { get; set; }
    /// <summary>ISO 4217 code when all completed orders share one currency; otherwise USD with <see cref="RevenueCurrencyMixed"/>.</summary>
    public string RevenueCurrencyCode { get; set; } = "USD";
    public bool RevenueCurrencyMixed { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    /// <summary>
    /// Per-currency revenue rollup over completed orders. Always populated (single
    /// entry when there is only one currency; multiple when mixed). Authoritative
    /// for UIs that want accountant-grade numbers without FX assumptions.
    /// </summary>
    public List<RevenueByCurrencyItemDto> RevenueByCurrency { get; set; } = new();
    public int TotalClients { get; set; }
    public int ActiveDesigners { get; set; }
    public int PendingOrders { get; set; }
    public int OrdersInProgress { get; set; }
    public int OrdersAwaitingAdminReview { get; set; }
    public int OrdersAwaitingClientApproval { get; set; }
    /// <summary>
    /// Approved orders that still have no designer assigned. Drives the SuperAdmin
    /// "Unassigned Orders Alert" widget and the orders grid filter.
    /// </summary>
    public int OrdersAwaitingDesignerAssignment { get; set; }
    public decimal RevisionRate { get; set; }
    public decimal ApprovalRate { get; set; }
    public double AverageDeliveryTimeDays { get; set; }
    public int OverdueOrders { get; set; }
}

/// <summary>Revenue rollup for a single ISO 4217 currency across completed orders.</summary>
public class RevenueByCurrencyItemDto
{
    public string CurrencyCode { get; set; } = "USD";
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int CompletedCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class OrderAnalyticsDto
{
    public List<OrdersTrendItemDto> OrdersTrend { get; set; } = new();
    public List<OrdersTrendDailyItemDto> OrdersTrendDaily { get; set; } = new();
    public List<OrdersByStatusItemDto> OrdersByStatus { get; set; } = new();
    public List<OrdersByPackageItemDto> OrdersByPackage { get; set; } = new();
    public List<OrdersByDayOfWeekItemDto> OrdersByDayOfWeek { get; set; } = new();
    public List<OrdersByHourItemDto> OrdersByHour { get; set; } = new();
    public List<OrdersByClientItemDto> OrdersByClient { get; set; } = new();
    public List<OrdersByDesignerItemDto> OrdersByDesigner { get; set; } = new();
    public List<DailyActivityItemDto> DailyActivity { get; set; } = new();
}

public class OrdersTrendDailyItemDto
{
    public string DateKey { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OrdersByClientItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class OrdersByDesignerItemDto
{
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class DailyActivityItemDto
{
    public string Date { get; set; } = string.Empty;
    public string DateKey { get; set; } = string.Empty;
    public int OrdersCreated { get; set; }
    public int FilesUploaded { get; set; }
    public int RevisionsRequested { get; set; }
    public int ApprovalsCompleted { get; set; }
}

public class OrdersTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CompletedCount { get; set; }
}

public class OrdersByStatusItemDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OrdersByPackageItemDto
{
    public string Package { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OrdersByDayOfWeekItemDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int DayIndex { get; set; }
    public int Count { get; set; }
}

public class OrdersByHourItemDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class RevenueAnalyticsDto
{
    public List<RevenueTrendItemDto> RevenueTrend { get; set; } = new();
    public List<RevenueTrendDailyItemDto> RevenueTrendDaily { get; set; } = new();
    public List<RevenueByPackageItemDto> RevenueByPackage { get; set; } = new();
    public List<AverageOrderValueTrendItemDto> AverageOrderValueTrend { get; set; } = new();
    public List<TopClientByRevenueDto> TopClientsByRevenue { get; set; } = new();
    public decimal RevenueGrowthRate { get; set; }
    public string RevenueCurrencyCode { get; set; } = "USD";
    public bool RevenueCurrencyMixed { get; set; }
}

public class RevenueTrendDailyItemDto
{
    public string DateKey { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class RevenueTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class RevenueByPackageItemDto
{
    public string Package { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class AverageOrderValueTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public decimal AverageOrderValue { get; set; }
}

public class TopClientByRevenueDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class DesignerAnalyticsDto
{
    public List<DesignerPerformanceItemDto> DesignerPerformance { get; set; } = new();
    public List<DesignerActivityTimelineItemDto> DesignerActivityTimeline { get; set; } = new();
}

public class DesignerActivityTimelineItemDto
{
    public string DateKey { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public int OrdersCompleted { get; set; }
    public int FilesUploaded { get; set; }
}

public class DesignerPerformanceItemDto
{
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public int OrdersCompleted { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal RevisionRate { get; set; }
    public double AverageCompletionTimeDays { get; set; }
    public int CurrentWorkload { get; set; }
}

public class ClientAnalyticsDto
{
    public int NewClients { get; set; }
    public int ReturningClients { get; set; }
    public List<OrdersPerClientItemDto> OrdersPerClient { get; set; } = new();
    public List<ClientRetentionTrendItemDto> ClientRetentionTrend { get; set; } = new();
    public List<TopClientByOrdersItemDto> TopClientsByOrders { get; set; } = new();
    public List<ClientLifetimeValueItemDto> ClientLifetimeValue { get; set; } = new();
}

public class TopClientByOrdersItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class ClientLifetimeValueItemDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class OrdersPerClientItemDto
{
    public string ClientName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class ClientRetentionTrendItemDto
{
    public string Month { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public int NewClients { get; set; }
    public int ReturningOrders { get; set; }
}

public class WorkflowAnalyticsDto
{
    public OrderFunnelDto OrderFunnel { get; set; } = new();
    public double AverageOrderCompletionTimeDays { get; set; }
    public List<RevisionDistributionItemDto> RevisionDistribution { get; set; } = new();
    public double AverageRevisionsPerOrder { get; set; }
    public RevisionQualityDto RevisionQuality { get; set; } = new();
    public List<OrdersStuckInStageItemDto> OrdersStuckInStage { get; set; } = new();
}

public class RevisionQualityDto
{
    public int OrdersWithNoRevisions { get; set; }
    public int OrdersWith1Revision { get; set; }
    public int OrdersWith2PlusRevisions { get; set; }
}

public class OrdersStuckInStageItemDto
{
    public string Stage { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OrderFunnelDto
{
    public int OrdersCreated { get; set; }
    public int AssignedToDesigner { get; set; }
    public int DesignSubmitted { get; set; }
    public int RevisionRequested { get; set; }
    public int ClientApproval { get; set; }
    public int Completed { get; set; }
}

public class RevisionDistributionItemDto
{
    public int RevisionCount { get; set; }
    public int OrderCount { get; set; }
}

public class SystemAnalyticsDto
{
    public List<DailyActivityItemDto> DailyActivity { get; set; } = new();
    public List<NotificationActivityItemDto> NotificationActivity { get; set; } = new();
    public FileUploadAnalyticsDto FileUploadAnalytics { get; set; } = new();
}

public class NotificationActivityItemDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class FileUploadAnalyticsDto
{
    public int TotalUploads { get; set; }
    public List<FileUploadByDayItemDto> UploadsByDay { get; set; } = new();
    public List<FileUploadByTypeItemDto> UploadsByType { get; set; } = new();
}

public class FileUploadByDayItemDto
{
    public string DateKey { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class FileUploadByTypeItemDto
{
    public string FileType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ForecastAnalyticsDto
{
    public List<OrderForecastItemDto> OrderGrowthPrediction { get; set; } = new();
    public List<RevenueForecastItemDto> RevenueForecast { get; set; } = new();
    public string RevenueCurrencyCode { get; set; } = "USD";
    public bool RevenueCurrencyMixed { get; set; }
}

public class OrderForecastItemDto
{
    public string PeriodKey { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public int Actual { get; set; }
    public double? Predicted { get; set; }
}

public class RevenueForecastItemDto
{
    public string PeriodKey { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal Actual { get; set; }
    public decimal? Predicted { get; set; }
}

public class InsightsAnalyticsDto
{
    public List<string> Insights { get; set; } = new();
}
