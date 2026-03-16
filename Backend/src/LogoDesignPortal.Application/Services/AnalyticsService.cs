using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;

    private static string GetPackageFromPriceForRevenue(decimal price)
    {
        if (price < 200) return "Basic";
        if (price < 500) return "Standard";
        if (price < 1000) return "Premium";
        return "Custom";
    }

    public AnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var totalRevenue = completedOrders.Sum(o => o.Price);
        var totalOrders = orders.Count;
        var completedCount = completedOrders.Count;

        var averageDeliveryTime = 0.0;
        var completedWithDates = completedOrders
            .Where(o => o.UpdatedAt.HasValue || o.CreatedAt != default)
            .ToList();
        if (completedWithDates.Count > 0)
        {
            var times = completedWithDates.Select(o =>
            {
                var completed = o.UpdatedAt ?? o.CreatedAt;
                return (completed - o.CreatedAt).TotalDays;
            }).Where(d => d >= 0).ToList();
            if (times.Count > 0)
                averageDeliveryTime = times.Average();
        }

        var ordersWithRevisions = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrderId)
            .Distinct()
            .CountAsync();
        var revisionRate = totalOrders > 0 ? (decimal)ordersWithRevisions / totalOrders * 100 : 0;
        var approvalRate = totalOrders > 0 ? (decimal)completedCount / totalOrders * 100 : 0;

        var monthlyRevenue = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .Sum(o => o.Price);

        var totalClients = await _context.ClientProfiles.CountAsync(c => !c.IsDeleted);
        var activeDesigners = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .Select(o => o.DesignerId!.Value)
            .Distinct()
            .CountAsync();

        var pendingOrders = orders.Count(o => o.Status == OrderStatus.WaitingForAdminApproval);
        var inProgress = orders.Count(o => o.Status == OrderStatus.InProgress || o.Status == OrderStatus.RevisionRequested);
        var awaitingAdminReview = orders.Count(o => o.Status == OrderStatus.WaitingForAdminApproval || o.Status == OrderStatus.PriceApprovalPending);
        var awaitingClientApproval = orders.Count(o => o.Status == OrderStatus.PreviewDelivered);
        var overdueOrders = orders.Count(o => o.Deadline.HasValue && o.Deadline.Value < now && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

        return new AnalyticsOverviewDto
        {
            TotalOrders = totalOrders,
            OrdersToday = orders.Count(o => o.CreatedAt >= startOfToday),
            OrdersThisMonth = orders.Count(o => o.CreatedAt >= startOfMonth),
            OrdersThisYear = orders.Count(o => o.CreatedAt >= startOfYear),
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            AverageOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0,
            TotalClients = totalClients,
            ActiveDesigners = activeDesigners,
            PendingOrders = pendingOrders,
            OrdersInProgress = inProgress,
            OrdersAwaitingAdminReview = awaitingAdminReview,
            OrdersAwaitingClientApproval = awaitingClientApproval,
            RevisionRate = Math.Round(revisionRate, 0),
            ApprovalRate = Math.Round(approvalRate, 0),
            AverageDeliveryTimeDays = Math.Round(averageDeliveryTime, 1),
            OverdueOrders = overdueOrders
        };
    }

    public async Task<OrderAnalyticsDto> GetOrderAnalyticsAsync()
    {
        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.Id, o.CreatedAt, o.Status, o.ClientId, o.DesignerId })
            .ToListAsync();

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var trendRaw = orders
            .Where(o => o.CreatedAt >= startOfSixMonths)
            .GroupBy(o => new { Year = o.CreatedAt.Year, Month = o.CreatedAt.Month })
            .Select(g => new
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Count = g.Count(),
                CompletedCount = g.Count(x => x.Status == OrderStatus.Completed)
            })
            .ToDictionary(x => x.MonthKey);

        // Ensure all 6 months are present (even if empty)
        var trend = new List<OrdersTrendItemDto>();
        for (var i = 5; i >= 0; i--)
        {
            var d = startOfSixMonths.AddMonths(i);
            var monthKey = $"{d.Year}-{d.Month:D2}";
            var item = trendRaw.TryGetValue(monthKey, out var raw)
                ? new OrdersTrendItemDto { MonthKey = monthKey, Month = raw.Month, Count = raw.Count, CompletedCount = raw.CompletedCount }
                : new OrdersTrendItemDto { MonthKey = monthKey, Month = d.ToString("MMM yyyy"), Count = 0, CompletedCount = 0 };
            trend.Add(item);
        }

        var byStatus = orders
            .GroupBy(o => o.Status.ToString())
            .Select(g => new OrdersByStatusItemDto { Status = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var byPackage = orders
            .GroupBy(_ => "Standard")
            .Select(g => new OrdersByPackageItemDto { Package = g.Key, Count = g.Count() })
            .ToList();

        var byDayOfWeek = orders
            .GroupBy(o => (int)o.CreatedAt.DayOfWeek)
            .Select(g => new OrdersByDayOfWeekItemDto
            {
                DayIndex = g.Key,
                DayOfWeek = Enum.GetName(typeof(DayOfWeek), g.Key) ?? "",
                Count = g.Count()
            })
            .OrderBy(x => x.DayIndex)
            .ToList();

        var byHour = orders
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new OrdersByHourItemDto { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToList();

        var last30Days = DateTime.UtcNow.AddDays(-30);
        var ordersTrendDaily = orders
            .Where(o => o.CreatedAt >= last30Days)
            .GroupBy(o => o.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new OrdersTrendDailyItemDto
            {
                DateKey = g.Key.ToString("yyyy-MM-dd"),
                Date = g.Key.ToString("MMM d"),
                Count = g.Count()
            })
            .ToList();

        var ordersByClient = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .Include(o => o.Client)
            .ThenInclude(c => c!.User)
            .GroupBy(o => o.ClientId)
            .Select(g => new OrdersByClientItemDto
            {
                ClientId = g.Key,
                ClientName = g.First().Client != null
                    ? (g.First().Client!.User != null ? $"{g.First().Client!.User!.FirstName} {g.First().Client!.User!.LastName} ({g.First().Client!.CompanyName})" : g.First().Client!.CompanyName)
                    : "Unknown",
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToListAsync();

        var ordersByDesigner = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue)
            .Include(o => o.Designer)
            .ThenInclude(d => d!.User)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new OrdersByDesignerItemDto
            {
                DesignerId = g.Key,
                DesignerName = g.First().Designer != null && g.First().Designer!.User != null
                    ? $"{g.First().Designer!.User!.FirstName} {g.First().Designer!.User!.LastName}"
                    : "Unknown",
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToListAsync();

        var last14Days = DateTime.UtcNow.AddDays(-14);
        var filesByDay = await _context.LogoFiles
            .Where(f => !f.IsDeleted && f.CreatedAt >= last14Days)
            .GroupBy(f => f.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var revisionsByDay = await _context.OrderRevisions
            .Where(r => !r.IsDeleted && r.CreatedAt >= last14Days)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var statusHistoryByDay = await _context.OrderStatusHistories
            .Where(h => h.CreatedAt >= last14Days)
            .Where(h => h.NewStatus == OrderStatus.Completed)
            .GroupBy(h => h.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var ordersByDay = orders
            .Where(o => o.CreatedAt >= last14Days)
            .GroupBy(o => o.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyActivity = new List<DailyActivityItemDto>();
        for (var i = 13; i >= 0; i--)
        {
            var d = DateTime.UtcNow.Date.AddDays(-i);
            var dateKey = d.ToString("yyyy-MM-dd");
            var filesCount = filesByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0;
            var revCount = revisionsByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0;
            var approvalCount = statusHistoryByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0;
            var orderCount = ordersByDay.TryGetValue(d, out var c) ? c : 0;
            dailyActivity.Add(new DailyActivityItemDto
            {
                DateKey = dateKey,
                Date = d.ToString("MMM d"),
                OrdersCreated = orderCount,
                FilesUploaded = filesCount,
                RevisionsRequested = revCount,
                ApprovalsCompleted = approvalCount
            });
        }

        return new OrderAnalyticsDto
        {
            OrdersTrend = trend,
            OrdersTrendDaily = ordersTrendDaily,
            OrdersByStatus = byStatus,
            OrdersByPackage = byPackage,
            OrdersByDayOfWeek = byDayOfWeek,
            OrdersByHour = byHour,
            OrdersByClient = ordersByClient,
            OrdersByDesigner = ordersByDesigner,
            DailyActivity = dailyActivity
        };
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync()
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Include(o => o.Client)
            .ThenInclude(c => c!.User)
            .Select(o => new { o.Price, o.CreatedAt, o.UpdatedAt, o.ClientId, o.Client })
            .ToListAsync();

        var revenueTrend = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new RevenueTrendItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Revenue = g.Sum(x => x.Price)
            })
            .ToList();

        // Group revenue by package type: Basic < 200, Standard < 500, Premium < 1000, Custom >= 1000
        var revenueByPackageRaw = completedOrders
            .GroupBy(o => GetPackageFromPriceForRevenue(o.Price))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));
        var packageOrder = new[] { "Basic", "Standard", "Premium", "Custom" };
        var revenueByPackage = packageOrder
            .Select(p => new RevenueByPackageItemDto { Package = p, Revenue = revenueByPackageRaw.GetValueOrDefault(p, 0) })
            .ToList();

        var aovTrend = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new AverageOrderValueTrendItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                AverageOrderValue = g.Count() > 0 ? g.Average(x => x.Price) : 0
            })
            .ToList();

        var topClients = completedOrders
            .GroupBy(o => o.ClientId)
            .Select(g =>
            {
                var first = g.First();
                var client = first.Client;
                var clientName = client != null
                    ? (client.User != null ? $"{client.User.FirstName} {client.User.LastName} ({client.CompanyName})" : client.CompanyName)
                    : "Unknown";
                return new
                {
                    ClientId = g.Key,
                    Revenue = g.Sum(x => x.Price),
                    OrderCount = g.Count(),
                    ClientName = clientName
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .Select(x => new TopClientByRevenueDto
            {
                ClientId = x.ClientId,
                ClientName = x.ClientName,
                Revenue = x.Revenue,
                OrderCount = x.OrderCount
            })
            .ToList();

        var last30Days = DateTime.UtcNow.AddDays(-30);
        var revenueTrendDaily = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= last30Days)
            .GroupBy(o => (o.UpdatedAt ?? o.CreatedAt).Date)
            .OrderBy(g => g.Key)
            .Select(g => new RevenueTrendDailyItemDto
            {
                DateKey = g.Key.ToString("yyyy-MM-dd"),
                Date = g.Key.ToString("MMM d"),
                Revenue = g.Sum(x => x.Price)
            })
            .ToList();

        var prevMonth = DateTime.UtcNow.AddMonths(-1);
        var prevMonthStart = new DateTime(prevMonth.Year, prevMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var thisMonthRevenue = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .Sum(o => o.Price);
        var lastMonthRevenue = completedOrders
            .Where(o =>
            {
                var d = o.UpdatedAt ?? o.CreatedAt;
                return d >= prevMonthStart && d < startOfMonth;
            })
            .Sum(o => o.Price);
        var revenueGrowthRate = lastMonthRevenue > 0
            ? (decimal)((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100)
            : 0;

        return new RevenueAnalyticsDto
        {
            RevenueTrend = revenueTrend,
            RevenueTrendDaily = revenueTrendDaily,
            RevenueByPackage = revenueByPackage,
            AverageOrderValueTrend = aovTrend,
            TopClientsByRevenue = topClients,
            RevenueGrowthRate = Math.Round(revenueGrowthRate, 1)
        };
    }

    public async Task<DesignerAnalyticsDto> GetDesignerAnalyticsAsync()
    {
        var designers = await _context.DesignerProfiles
            .Where(d => !d.IsDeleted)
            .Include(d => d.User)
            .ToListAsync();

        var ordersByDesigner = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue)
            .Select(o => new { o.DesignerId, o.Status, o.CreatedAt, o.UpdatedAt, o.Id })
            .ToListAsync();

        var revisionsByOrder = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .GroupBy(r => r.OrderId)
            .Select(g => new { OrderId = g.Key, Count = g.Count() })
            .ToListAsync();
        var revDict = revisionsByOrder.ToDictionary(x => x.OrderId, x => x.Count);

        var result = new List<DesignerPerformanceItemDto>();
        foreach (var d in designers)
        {
            var assigned = ordersByDesigner.Where(o => o.DesignerId == d.Id).ToList();
            var completed = assigned.Where(o => o.Status == OrderStatus.Completed).ToList();
            var withRevisions = assigned.Count(o => revDict.TryGetValue(o.Id, out var c) && c > 0);
            var approvalRate = assigned.Count > 0 ? (decimal)completed.Count / assigned.Count * 100 : 0;
            var revisionRate = assigned.Count > 0 ? (decimal)withRevisions / assigned.Count * 100 : 0;
            var avgCompletion = completed.Count > 0
                ? completed.Average(o =>
                {
                    var end = o.UpdatedAt ?? o.CreatedAt;
                    return (end - o.CreatedAt).TotalDays;
                })
                : 0;
            var workload = assigned.Count(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

            result.Add(new DesignerPerformanceItemDto
            {
                DesignerId = d.Id,
                DesignerName = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
                OrdersCompleted = completed.Count,
                ApprovalRate = Math.Round(approvalRate, 0),
                RevisionRate = Math.Round(revisionRate, 0),
                AverageCompletionTimeDays = Math.Round(avgCompletion, 1),
                CurrentWorkload = workload
            });
        }

        var last14Days = DateTime.UtcNow.AddDays(-14);
        var completedByDesignerDay = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue && o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= last14Days)
            .Select(o => new { Date = (o.UpdatedAt ?? o.CreatedAt).Date, o.DesignerId })
            .ToListAsync();

        var designerIds = completedByDesignerDay.Select(x => x.DesignerId!.Value).Distinct().ToList();
        var designerNameDict = new Dictionary<Guid, string>();
        if (designerIds.Count > 0)
        {
            var designersForNames = await _context.DesignerProfiles
                .Where(d => designerIds.Contains(d.Id) && !d.IsDeleted)
                .Include(d => d.User)
                .ToListAsync();
            foreach (var d in designersForNames)
            {
                designerNameDict[d.Id] = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown";
            }
        }

        var designerActivityList = completedByDesignerDay
            .GroupBy(x => new { x.Date, DesignerId = x.DesignerId!.Value })
            .Select(g =>
            {
                var name = designerNameDict.TryGetValue(g.Key.DesignerId, out var n) ? n : "Unknown";
                return new DesignerActivityTimelineItemDto
                {
                    DateKey = g.Key.Date.ToString("yyyy-MM-dd"),
                    Date = g.Key.Date.ToString("MMM d"),
                    DesignerId = g.Key.DesignerId,
                    DesignerName = name,
                    OrdersCompleted = g.Count(),
                    FilesUploaded = 0
                };
            })
            .OrderBy(x => x.DateKey)
            .Take(50)
            .ToList();

        return new DesignerAnalyticsDto
        {
            DesignerPerformance = result.OrderByDescending(x => x.OrdersCompleted).ToList(),
            DesignerActivityTimeline = designerActivityList
        };
    }

    public async Task<ClientAnalyticsDto> GetClientAnalyticsAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var clients = await _context.ClientProfiles
            .Where(c => !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Orders.Where(o => !o.IsDeleted))
            .ToListAsync();

        var firstOrderByClient = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, FirstOrder = g.Min(o => o.CreatedAt) })
            .ToListAsync();
        var firstOrderDict = firstOrderByClient.ToDictionary(x => x.ClientId, x => x.FirstOrder);

        var newClients = clients.Count(c => firstOrderDict.TryGetValue(c.Id, out var first) && first >= startOfSixMonths);
        var returningClients = clients.Count - newClients;

        var ordersPerClient = clients
            .Select(c => new OrdersPerClientItemDto
            {
                ClientName = c.User != null ? $"{c.User.FirstName} {c.User.LastName} ({c.CompanyName})" : c.CompanyName,
                OrderCount = c.Orders.Count
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToList();

        var retentionTrend = new List<ClientRetentionTrendItemDto>();
        for (var i = 5; i >= 0; i--)
        {
            var monthStart = startOfSixMonths.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var newInMonth = firstOrderByClient.Count(x => x.FirstOrder >= monthStart && x.FirstOrder < monthEnd);
            var ordersInMonth = await _context.LogoOrders
                .Where(o => !o.IsDeleted && o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
                .CountAsync();
            var returningInMonth = Math.Max(0, ordersInMonth - newInMonth);
            retentionTrend.Add(new ClientRetentionTrendItemDto
            {
                MonthKey = $"{monthStart.Year}-{monthStart.Month:D2}",
                Month = monthStart.ToString("MMM yyyy"),
                NewClients = newInMonth,
                ReturningOrders = returningInMonth
            });
        }

        var topClientsByOrders = clients
            .Select(c => new TopClientByOrdersItemDto
            {
                ClientId = c.Id,
                ClientName = c.User != null ? $"{c.User.FirstName} {c.User.LastName} ({c.CompanyName})" : c.CompanyName,
                OrderCount = c.Orders.Count
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(10)
            .ToList();

        var completedWithRevenue = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Include(o => o.Client)
            .ThenInclude(c => c!.User)
            .Select(o => new { o.ClientId, o.Price, o.Client })
            .ToListAsync();
        var clientLifetimeValue = completedWithRevenue
            .GroupBy(o => o.ClientId)
            .Select(g =>
            {
                var first = g.First();
                var name = first.Client != null
                    ? (first.Client.User != null ? $"{first.Client.User.FirstName} {first.Client.User.LastName} ({first.Client.CompanyName})" : first.Client.CompanyName)
                    : "Unknown";
                return new ClientLifetimeValueItemDto
                {
                    ClientId = g.Key,
                    ClientName = name,
                    Revenue = g.Sum(x => x.Price),
                    OrderCount = g.Count()
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        return new ClientAnalyticsDto
        {
            NewClients = newClients,
            ReturningClients = returningClients,
            OrdersPerClient = ordersPerClient,
            ClientRetentionTrend = retentionTrend,
            TopClientsByOrders = topClientsByOrders,
            ClientLifetimeValue = clientLifetimeValue
        };
    }

    public async Task<WorkflowAnalyticsDto> GetWorkflowAnalyticsAsync()
    {
        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        var ordersWithRevisions = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrderId)
            .Distinct()
            .CountAsync();

        var funnel = new OrderFunnelDto
        {
            OrdersCreated = orders.Count,
            AssignedToDesigner = orders.Count(o => o.DesignerId.HasValue),
            DesignSubmitted = orders.Count(o => o.Status == OrderStatus.PreviewDelivered || o.Status == OrderStatus.RevisionRequested || o.Status == OrderStatus.Completed),
            RevisionRequested = ordersWithRevisions,
            ClientApproval = orders.Count(o => o.Status == OrderStatus.PreviewDelivered || o.Status == OrderStatus.ClientApproved),
            Completed = orders.Count(o => o.Status == OrderStatus.Completed)
        };

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var avgCompletion = completedOrders.Count > 0
            ? completedOrders.Average(o =>
            {
                var end = o.UpdatedAt ?? o.CreatedAt;
                return (end - o.CreatedAt).TotalDays;
            })
            : 0;

        var revisionCounts = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .GroupBy(r => r.OrderId)
            .Select(g => new { OrderId = g.Key, Count = g.Count() })
            .ToListAsync();

        var revDist = revisionCounts
            .GroupBy(x => x.Count)
            .Select(g => new RevisionDistributionItemDto { RevisionCount = g.Key, OrderCount = g.Count() })
            .OrderBy(x => x.RevisionCount)
            .ToList();

        var orderIds = orders.Select(o => o.Id).ToHashSet();
        var relevantRevisions = revisionCounts.Where(x => orderIds.Contains(x.OrderId)).ToList();
        var avgRevisions = orders.Count > 0 && relevantRevisions.Count > 0
            ? relevantRevisions.Average(x => (double)x.Count)
            : 0;

        var revCountDict = relevantRevisions.ToDictionary(x => x.OrderId, x => x.Count);
        var ordersWithNoRevisions = orders.Count(o => !revCountDict.ContainsKey(o.Id) || revCountDict[o.Id] == 0);
        var ordersWith1Revision = orders.Count(o => revCountDict.TryGetValue(o.Id, out var c) && c == 1);
        var ordersWith2PlusRevisions = orders.Count(o => revCountDict.TryGetValue(o.Id, out var c) && c >= 2);

        var ordersStuckInStage = new List<OrdersStuckInStageItemDto>
        {
            new() { Stage = "Awaiting Admin Review", Count = orders.Count(o => o.Status == OrderStatus.WaitingForAdminApproval || o.Status == OrderStatus.PriceApprovalPending) },
            new() { Stage = "Unassigned", Count = orders.Count(o => !o.DesignerId.HasValue && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled) },
            new() { Stage = "In Progress", Count = orders.Count(o => o.Status == OrderStatus.InProgress) },
            new() { Stage = "Revision Requested", Count = orders.Count(o => o.Status == OrderStatus.RevisionRequested) },
            new() { Stage = "Awaiting Client Approval", Count = orders.Count(o => o.Status == OrderStatus.PreviewDelivered) }
        }.Where(x => x.Count > 0).ToList();

        return new WorkflowAnalyticsDto
        {
            OrderFunnel = funnel,
            AverageOrderCompletionTimeDays = Math.Round(avgCompletion, 1),
            RevisionDistribution = revDist,
            AverageRevisionsPerOrder = Math.Round(avgRevisions, 1),
            RevisionQuality = new RevisionQualityDto
            {
                OrdersWithNoRevisions = ordersWithNoRevisions,
                OrdersWith1Revision = ordersWith1Revision,
                OrdersWith2PlusRevisions = ordersWith2PlusRevisions
            },
            OrdersStuckInStage = ordersStuckInStage
        };
    }

    public async Task<SystemAnalyticsDto> GetSystemAnalyticsAsync()
    {
        var last14Days = DateTime.UtcNow.AddDays(-14);

        var filesByDay = await _context.LogoFiles
            .Where(f => !f.IsDeleted && f.CreatedAt >= last14Days)
            .GroupBy(f => f.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var revisionsByDay = await _context.OrderRevisions
            .Where(r => !r.IsDeleted && r.CreatedAt >= last14Days)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var approvalsByDay = await _context.OrderStatusHistories
            .Where(h => h.CreatedAt >= last14Days && h.NewStatus == OrderStatus.Completed)
            .GroupBy(h => h.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var ordersByDay = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.CreatedAt >= last14Days)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var notificationsByDay = await _context.Notifications
            .Where(n => n.CreatedAt >= last14Days)
            .GroupBy(n => n.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var dailyActivity = new List<DailyActivityItemDto>();
        for (var i = 13; i >= 0; i--)
        {
            var d = DateTime.UtcNow.Date.AddDays(-i);
            dailyActivity.Add(new DailyActivityItemDto
            {
                DateKey = d.ToString("yyyy-MM-dd"),
                Date = d.ToString("MMM d"),
                OrdersCreated = ordersByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                FilesUploaded = filesByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                RevisionsRequested = revisionsByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0,
                ApprovalsCompleted = approvalsByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0
            });
        }

        var notificationActivityRaw = await _context.Notifications
            .Where(n => n.CreatedAt >= last14Days)
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();
        var notificationActivity = notificationActivityRaw
            .OrderByDescending(x => x.Count)
            .Select(x => new NotificationActivityItemDto { Type = x.Type.ToString(), Count = x.Count })
            .ToList();

        var totalUploads = await _context.LogoFiles.CountAsync(f => !f.IsDeleted);
        var uploadsByDay = filesByDay
            .Select(x => new FileUploadByDayItemDto
            {
                DateKey = x.Date.ToString("yyyy-MM-dd"),
                Date = x.Date.ToString("MMM d"),
                Count = x.Count
            })
            .OrderBy(x => x.DateKey)
            .ToList();
        var uploadsByTypeRaw = await _context.LogoFiles
            .Where(f => !f.IsDeleted)
            .GroupBy(f => f.FileType)
            .Select(g => new { FileType = g.Key, Count = g.Count() })
            .ToListAsync();
        var uploadsByType = uploadsByTypeRaw
            .Select(x => new FileUploadByTypeItemDto { FileType = x.FileType.ToString(), Count = x.Count })
            .ToList();

        return new SystemAnalyticsDto
        {
            DailyActivity = dailyActivity,
            NotificationActivity = notificationActivity,
            FileUploadAnalytics = new FileUploadAnalyticsDto
            {
                TotalUploads = totalUploads,
                UploadsByDay = uploadsByDay,
                UploadsByType = uploadsByType
            }
        };
    }

    public async Task<ForecastAnalyticsDto> GetForecastAnalyticsAsync()
    {
        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.CreatedAt })
            .ToListAsync();
        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.CreatedAt, o.UpdatedAt })
            .ToListAsync();

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var orderTrend = orders
            .Where(o => o.CreatedAt >= startOfSixMonths)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}", Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), Count = g.Count() })
            .ToList();

        var revenueTrend = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}", Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), Revenue = g.Sum(x => x.Price) })
            .ToList();

        var orderValues = orderTrend.Select(x => (double)x.Count).ToList();
        var revenueValues = revenueTrend.Select(x => (double)x.Revenue).ToList();

        var orderPrediction = new List<OrderForecastItemDto>();
        for (var i = 0; i < orderTrend.Count; i++)
        {
            var actual = orderTrend[i].Count;
            double? predicted = null;
            if (i >= 2)
            {
                var window = orderValues.Skip(i - 2).Take(2).ToList();
                predicted = Math.Round(window.Average(), 0);
            }
            orderPrediction.Add(new OrderForecastItemDto
            {
                PeriodKey = orderTrend[i].MonthKey,
                Period = orderTrend[i].Month,
                Actual = actual,
                Predicted = predicted
            });
        }
        if (orderValues.Count >= 2)
        {
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            orderPrediction.Add(new OrderForecastItemDto
            {
                PeriodKey = $"{nextMonth.Year}-{nextMonth.Month:D2}",
                Period = nextMonth.ToString("MMM yyyy"),
                Actual = 0,
                Predicted = Math.Round(orderValues.TakeLast(2).Average(), 0)
            });
        }

        var revenuePrediction = new List<RevenueForecastItemDto>();
        for (var i = 0; i < revenueTrend.Count; i++)
        {
            var actual = revenueTrend[i].Revenue;
            decimal? predicted = null;
            if (i >= 2)
            {
                var window = revenueValues.Skip(i - 2).Take(2).ToList();
                predicted = (decimal)Math.Round(window.Average(), 2);
            }
            revenuePrediction.Add(new RevenueForecastItemDto
            {
                PeriodKey = revenueTrend[i].MonthKey,
                Period = revenueTrend[i].Month,
                Actual = actual,
                Predicted = predicted
            });
        }
        if (revenueValues.Count >= 2)
        {
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            revenuePrediction.Add(new RevenueForecastItemDto
            {
                PeriodKey = $"{nextMonth.Year}-{nextMonth.Month:D2}",
                Period = nextMonth.ToString("MMM yyyy"),
                Actual = 0,
                Predicted = (decimal)Math.Round(revenueValues.TakeLast(2).Average(), 2)
            });
        }

        return new ForecastAnalyticsDto
        {
            OrderGrowthPrediction = orderPrediction,
            RevenueForecast = revenuePrediction
        };
    }

    public async Task<InsightsAnalyticsDto> GetInsightsAsync()
    {
        var insights = new List<string>();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var orders = await _context.LogoOrders.Where(o => !o.IsDeleted).ToListAsync();
        var ordersThisMonth = orders.Count(o => o.CreatedAt >= startOfMonth);
        var ordersLastMonth = orders.Count(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt < startOfMonth);
        if (ordersLastMonth > 0)
        {
            var pctChange = (ordersThisMonth - ordersLastMonth) * 100.0 / ordersLastMonth;
            insights.Add($"Orders {(pctChange >= 0 ? "increased" : "decreased")} {Math.Abs(Math.Round(pctChange, 0))}% this month compared to last month.");
        }

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var totalRevenue = completedOrders.Sum(o => o.Price);
        var revenueThisMonth = completedOrders.Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth).Sum(o => o.Price);
        var revenueLastMonth = completedOrders.Where(o =>
        {
            var d = o.UpdatedAt ?? o.CreatedAt;
            return d >= startOfLastMonth && d < startOfMonth;
        }).Sum(o => o.Price);
        if (revenueLastMonth > 0)
        {
            var pctChange = (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100);
            insights.Add($"Revenue {(pctChange >= 0 ? "increased" : "decreased")} {Math.Abs(Math.Round(pctChange, 0))}% this month.");
        }

        var designerProfiles = await _context.DesignerProfiles.Where(d => !d.IsDeleted).Include(d => d.User).ToListAsync();
        var ordersByDesigner = orders.Where(o => o.DesignerId.HasValue).GroupBy(o => o.DesignerId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var revisionCounts = await _context.OrderRevisions.Where(r => !r.IsDeleted).GroupBy(r => r.OrderId).Select(g => new { OrderId = g.Key, Count = g.Count() }).ToListAsync();
        var revDict = revisionCounts.ToDictionary(x => x.OrderId, x => x.Count);

        DesignerProfile? topDesigner = null;
        decimal topApprovalRate = 0;
        foreach (var d in designerProfiles)
        {
            if (!ordersByDesigner.TryGetValue(d.Id, out var assigned)) continue;
            var completed = assigned.Count(o => o.Status == OrderStatus.Completed);
            if (assigned.Count > 0)
            {
                var rate = (decimal)completed / assigned.Count * 100;
                if (rate > topApprovalRate && assigned.Count >= 3)
                {
                    topApprovalRate = rate;
                    topDesigner = d;
                }
            }
        }
        if (topDesigner != null && topDesigner.User != null)
        {
            insights.Add($"{topDesigner.User.FirstName} {topDesigner.User.LastName} has the highest approval rate ({Math.Round(topApprovalRate, 0)}%) among designers with 3+ orders.");
        }

        var ordersWithRevisions = revDict.Count;
        var revisionPct = orders.Count > 0 ? (decimal)ordersWithRevisions / orders.Count * 100 : 0;
        insights.Add($"{Math.Round(revisionPct, 0)}% of orders require revisions.");

        if (revenueThisMonth > 0 && totalRevenue > 0)
        {
            var premiumPct = revenueThisMonth / totalRevenue * 100;
            if (premiumPct > 50)
                insights.Add("Premium/Standard package revenue represents the majority of monthly revenue.");
        }

        return new InsightsAnalyticsDto { Insights = insights };
    }
}
