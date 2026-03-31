using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _distributedCache;
    private readonly IReadModelCacheVersions _cacheVersions;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IApplicationDbContext context,
        IDistributedCache distributedCache,
        IReadModelCacheVersions cacheVersions,
        ILogger<AnalyticsService> logger)
    {
        _context = context;
        _distributedCache = distributedCache;
        _cacheVersions = cacheVersions;
        _logger = logger;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync()
    {
        // Dashboard aggregate: 5–10 min TTL; invalidated via IReadModelCacheVersions on writes.
        var cacheKey = $"ldp:cache:analytics:overview:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<AnalyticsOverviewDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var dto = await BuildOverviewUncachedAsync();
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    /// <summary>
    /// Computes overview using DB aggregates and projections (avoids materializing all orders).
    /// </summary>
    private async Task<AnalyticsOverviewDto> BuildOverviewUncachedAsync()
    {
        var now = DateTime.UtcNow;
        var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var q = _context.LogoOrders.AsNoTracking().Where(o => !o.IsDeleted);

        var totalOrders = await q.CountAsync();
        var completedCount = await q.CountAsync(o => o.Status == OrderStatus.Completed);
        var totalRevenue = completedCount == 0
            ? 0m
            : await q.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => o.Price);

        var averageDeliveryTime = 0.0;
        var completedForAvg = q
            .Where(o => o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= o.CreatedAt);
        if (await completedForAvg.AnyAsync().ConfigureAwait(false))
        {
            // Fetch timestamps only; compute duration in memory — EF cannot translate TimeSpan.TotalDays reliably (throws DateTime vs TimeSpan? coercion on some providers).
            var deliveryRows = await completedForAvg
                .Select(o => new { o.CreatedAt, End = o.UpdatedAt ?? o.CreatedAt })
                .Where(x => x.End >= x.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
            if (deliveryRows.Count > 0)
                averageDeliveryTime = deliveryRows.Average(x => (x.End - x.CreatedAt).TotalDays);
        }

        var ordersWithRevisions = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrderId)
            .Distinct()
            .CountAsync();
        var revisionRate = totalOrders > 0 ? (decimal)ordersWithRevisions / totalOrders * 100 : 0;
        var approvalRate = totalOrders > 0 ? (decimal)completedCount / totalOrders * 100 : 0;

        var monthlyRevenue = await q
            .Where(o => o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .SumAsync(o => o.Price);

        var totalClients = await _context.ClientProfiles.CountAsync(c => !c.IsDeleted);
        var activeDesigners = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .Select(o => o.DesignerId!.Value)
            .Distinct()
            .CountAsync();

        var pendingOrders = await q.CountAsync(o => o.Status == OrderStatus.WaitingForAdminApproval);
        var inProgress = await q.CountAsync(o => o.Status == OrderStatus.InProgress || o.Status == OrderStatus.RevisionRequested);
        var awaitingAdminReview = await q.CountAsync(o => o.Status == OrderStatus.WaitingForAdminApproval || o.Status == OrderStatus.PriceApprovalPending);
        var awaitingClientApproval = await q.CountAsync(o => o.Status == OrderStatus.PreviewDelivered);
        var overdueOrders = await q.CountAsync(o =>
            o.Deadline.HasValue && o.Deadline.Value < now && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

        return new AnalyticsOverviewDto
        {
            TotalOrders = totalOrders,
            OrdersToday = await q.CountAsync(o => o.CreatedAt >= startOfToday),
            OrdersThisMonth = await q.CountAsync(o => o.CreatedAt >= startOfMonth),
            OrdersThisYear = await q.CountAsync(o => o.CreatedAt >= startOfYear),
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
        var cacheKey = $"ldp:cache:analytics:orders:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<OrderAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var q = _context.LogoOrders.AsNoTracking().Where(o => !o.IsDeleted);

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var trendRows = await q
            .Where(o => o.CreatedAt >= startOfSixMonths)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count(),
                CompletedCount = g.Count(x => x.Status == OrderStatus.Completed)
            })
            .ToListAsync();
        var trendRaw = trendRows.ToDictionary(
            x => $"{x.Year}-{x.Month:D2}",
            x => (Month: new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), x.Count, x.CompletedCount));

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

        var byStatus = await q
            .GroupBy(o => o.Status)
            .Select(g => new OrdersByStatusItemDto { Status = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var totalForPackage = await q.CountAsync();
        var byPackage = new List<OrdersByPackageItemDto>
        {
            new() { Package = "Standard", Count = totalForPackage }
        };

        var byDayOfWeek = await q
            .GroupBy(o => (int)o.CreatedAt.DayOfWeek)
            .Select(g => new OrdersByDayOfWeekItemDto
            {
                DayIndex = g.Key,
                DayOfWeek = "",
                Count = g.Count()
            })
            .OrderBy(x => x.DayIndex)
            .ToListAsync();
        foreach (var x in byDayOfWeek)
            x.DayOfWeek = Enum.GetName(typeof(DayOfWeek), x.DayIndex) ?? "";

        var byHour = await q
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new OrdersByHourItemDto { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();

        var last30Days = DateTime.UtcNow.AddDays(-30);
        var dailyTrendRaw = await q
            .Where(o => o.CreatedAt >= last30Days)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();
        var ordersTrendDaily = dailyTrendRaw
            .Select(x => new OrdersTrendDailyItemDto
            {
                DateKey = x.Date.ToString("yyyy-MM-dd"),
                Date = x.Date.ToString("MMM d"),
                Count = x.Count
            })
            .ToList();

        var clientAgg = await q
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, OrderCount = g.Count() })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToListAsync();
        var clientIds = clientAgg.Select(x => x.ClientId).ToList();
        var clientsInfo = await _context.ClientProfiles.AsNoTracking()
            .Where(c => clientIds.Contains(c.Id) && !c.IsDeleted)
            .Select(c => new { c.Id, c.CompanyName, c.User!.FirstName, c.User.LastName })
            .ToListAsync();
        var clientLookup = clientsInfo.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace($"{x.FirstName} {x.LastName}".Trim())
                ? (x.CompanyName ?? "Unknown")
                : $"{x.FirstName} {x.LastName} ({x.CompanyName})");
        var ordersByClient = clientAgg
            .Select(x => new OrdersByClientItemDto
            {
                ClientId = x.ClientId,
                ClientName = clientLookup.TryGetValue(x.ClientId, out var cn) ? cn : "Unknown",
                OrderCount = x.OrderCount
            })
            .ToList();

        var designerAgg = await q
            .Where(o => o.DesignerId.HasValue)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new { DesignerId = g.Key, OrderCount = g.Count() })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToListAsync();
        var designerIds = designerAgg.Select(x => x.DesignerId).ToList();
        var designersInfo = await _context.DesignerProfiles.AsNoTracking()
            .Where(d => designerIds.Contains(d.Id) && !d.IsDeleted)
            .Select(d => new { d.Id, d.User!.FirstName, d.User.LastName })
            .ToListAsync();
        var designerLookup = designersInfo.ToDictionary(x => x.Id, x => $"{x.FirstName} {x.LastName}".Trim());
        var ordersByDesigner = designerAgg
            .Select(x => new OrdersByDesignerItemDto
            {
                DesignerId = x.DesignerId,
                DesignerName = designerLookup.TryGetValue(x.DesignerId, out var dn) && !string.IsNullOrWhiteSpace(dn) ? dn : "Unknown",
                OrderCount = x.OrderCount
            })
            .ToList();

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

        var ordersByDayList = await q
            .Where(o => o.CreatedAt >= last14Days)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var ordersByDay = ordersByDayList.ToDictionary(x => x.Date, x => x.Count);

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

        var dto = new OrderAnalyticsDto
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
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync()
    {
        var cacheKey = $"ldp:cache:analytics:revenue:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<RevenueAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var last30Days = now.AddDays(-30);
        var prevMonth = now.AddMonths(-1);
        var prevMonthStart = new DateTime(prevMonth.Year, prevMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var q = _context.LogoOrders.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);

        var revenueTrendRaw = await q
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Y = (o.UpdatedAt ?? o.CreatedAt).Year, M = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new { g.Key.Y, g.Key.M, Revenue = g.Sum(x => x.Price) })
            .OrderBy(x => x.Y).ThenBy(x => x.M)
            .ToListAsync()
            .ConfigureAwait(false);
        var revenueTrend = revenueTrendRaw.Select(g => new RevenueTrendItemDto
        {
            MonthKey = $"{g.Y}-{g.M:D2}",
            Month = new DateTime(g.Y, g.M, 1).ToString("MMM yyyy"),
            Revenue = g.Revenue
        }).ToList();

        var basicRev = await q.Where(o => o.Price < 200m).SumAsync(o => o.Price).ConfigureAwait(false);
        var stdRev = await q.Where(o => o.Price >= 200m && o.Price < 500m).SumAsync(o => o.Price).ConfigureAwait(false);
        var premRev = await q.Where(o => o.Price >= 500m && o.Price < 1000m).SumAsync(o => o.Price).ConfigureAwait(false);
        var customRev = await q.Where(o => o.Price >= 1000m).SumAsync(o => o.Price).ConfigureAwait(false);
        var revenueByPackage = new List<RevenueByPackageItemDto>
        {
            new() { Package = "Basic", Revenue = basicRev },
            new() { Package = "Standard", Revenue = stdRev },
            new() { Package = "Premium", Revenue = premRev },
            new() { Package = "Custom", Revenue = customRev }
        };

        var aovRaw = await q
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Y = (o.UpdatedAt ?? o.CreatedAt).Year, M = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new { g.Key.Y, g.Key.M, AverageOrderValue = g.Average(x => x.Price) })
            .OrderBy(x => x.Y).ThenBy(x => x.M)
            .ToListAsync()
            .ConfigureAwait(false);
        var aovTrend = aovRaw.Select(g => new AverageOrderValueTrendItemDto
        {
            MonthKey = $"{g.Y}-{g.M:D2}",
            Month = new DateTime(g.Y, g.M, 1).ToString("MMM yyyy"),
            AverageOrderValue = g.AverageOrderValue
        }).ToList();

        var topClientRows = await q
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Revenue = g.Sum(x => x.Price), OrderCount = g.Count() })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync()
            .ConfigureAwait(false);
        var topClientIds = topClientRows.Select(x => x.ClientId).ToList();
        var topClientNames = await _context.ClientProfiles.AsNoTracking()
            .Where(c => topClientIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CompanyName, c.User!.FirstName, c.User.LastName })
            .ToListAsync()
            .ConfigureAwait(false);
        var topNameLookup = topClientNames.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace($"{x.FirstName} {x.LastName}".Trim())
                ? (x.CompanyName ?? "Unknown")
                : $"{x.FirstName} {x.LastName} ({x.CompanyName})");
        var topClients = topClientRows.Select(x => new TopClientByRevenueDto
        {
            ClientId = x.ClientId,
            ClientName = topNameLookup.TryGetValue(x.ClientId, out var nm) ? nm : "Unknown",
            Revenue = x.Revenue,
            OrderCount = x.OrderCount
        }).ToList();

        var dailyRaw = await q
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= last30Days)
            .GroupBy(o => (o.UpdatedAt ?? o.CreatedAt).Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Price) })
            .OrderBy(x => x.Date)
            .ToListAsync()
            .ConfigureAwait(false);
        var revenueTrendDaily = dailyRaw.Select(g => new RevenueTrendDailyItemDto
        {
            DateKey = g.Date.ToString("yyyy-MM-dd"),
            Date = g.Date.ToString("MMM d"),
            Revenue = g.Revenue
        }).ToList();

        var thisMonthRevenue = await q
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .SumAsync(o => o.Price)
            .ConfigureAwait(false);
        var lastMonthRevenue = await q
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= prevMonthStart && (o.UpdatedAt ?? o.CreatedAt) < startOfMonth)
            .SumAsync(o => o.Price)
            .ConfigureAwait(false);
        var revenueGrowthRate = lastMonthRevenue > 0
            ? (decimal)((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100)
            : 0;

        var dto = new RevenueAnalyticsDto
        {
            RevenueTrend = revenueTrend,
            RevenueTrendDaily = revenueTrendDaily,
            RevenueByPackage = revenueByPackage,
            AverageOrderValueTrend = aovTrend,
            TopClientsByRevenue = topClients,
            RevenueGrowthRate = Math.Round(revenueGrowthRate, 1)
        };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<DesignerAnalyticsDto> GetDesignerAnalyticsAsync()
    {
        var cacheKey = $"ldp:cache:analytics:designers:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<DesignerAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var designers = await _context.DesignerProfiles.AsNoTracking()
            .Select(d => new { d.Id, FirstName = d.User != null ? d.User.FirstName : "", LastName = d.User != null ? d.User.LastName : "" })
            .ToListAsync()
            .ConfigureAwait(false);

        var stats = await _context.LogoOrders.AsNoTracking()
            .Where(o => o.DesignerId.HasValue)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new
            {
                DesignerId = g.Key,
                TotalAssigned = g.Count(),
                Completed = g.Count(x => x.Status == OrderStatus.Completed),
                Workload = g.Count(x => x.Status != OrderStatus.Completed && x.Status != OrderStatus.Cancelled)
            })
            .ToListAsync()
            .ConfigureAwait(false);
        var statsDict = stats.ToDictionary(x => x.DesignerId);

        var withRev = await _context.LogoOrders.AsNoTracking()
            .Where(o => o.DesignerId.HasValue)
            .Where(o => _context.OrderRevisions.Any(r => r.OrderId == o.Id && !r.IsDeleted))
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new { DesignerId = g.Key, WithRevisions = g.Count() })
            .ToListAsync()
            .ConfigureAwait(false);
        var withRevDict = withRev.ToDictionary(x => x.DesignerId, x => x.WithRevisions);

        var avgDict = await _context.GetDesignerAverageCompletionDaysByDesignerAsync()
            .ConfigureAwait(false);

        var result = new List<DesignerPerformanceItemDto>();
        foreach (var d in designers)
        {
            statsDict.TryGetValue(d.Id, out var s);
            var total = s?.TotalAssigned ?? 0;
            var completedCnt = s?.Completed ?? 0;
            var withRevisionOrders = withRevDict.TryGetValue(d.Id, out var wr) ? wr : 0;
            var approvalRate = total > 0 ? (decimal)completedCnt / total * 100 : 0;
            var revisionRate = total > 0 ? (decimal)withRevisionOrders / total * 100 : 0;
            avgDict.TryGetValue(d.Id, out var avgCompletion);

            result.Add(new DesignerPerformanceItemDto
            {
                DesignerId = d.Id,
                DesignerName = !string.IsNullOrWhiteSpace($"{d.FirstName} {d.LastName}".Trim())
                    ? $"{d.FirstName} {d.LastName}".Trim()
                    : "Unknown",
                OrdersCompleted = completedCnt,
                ApprovalRate = Math.Round(approvalRate, 0),
                RevisionRate = Math.Round(revisionRate, 0),
                AverageCompletionTimeDays = Math.Round(avgCompletion, 1),
                CurrentWorkload = s?.Workload ?? 0
            });
        }

        var last14Days = DateTime.UtcNow.AddDays(-14);
        var timelineAgg = await _context.LogoOrders.AsNoTracking()
            .Where(o => o.DesignerId.HasValue && o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= last14Days)
            .GroupBy(o => new { Date = (o.UpdatedAt ?? o.CreatedAt).Date, DesignerId = o.DesignerId!.Value })
            .Select(g => new { g.Key.Date, g.Key.DesignerId, OrdersCompleted = g.Count() })
            .OrderBy(x => x.Date).ThenBy(x => x.DesignerId)
            .ToListAsync()
            .ConfigureAwait(false);

        var timelineDesignerIds = timelineAgg.Select(x => x.DesignerId).Distinct().ToList();
        Dictionary<Guid, string> timelineNames = new();
        if (timelineDesignerIds.Count > 0)
        {
            var timelineRows = await _context.DesignerProfiles.AsNoTracking()
                .Where(d => timelineDesignerIds.Contains(d.Id))
                .Select(d => new
                {
                    d.Id,
                    FirstName = d.User != null ? d.User.FirstName : "",
                    LastName = d.User != null ? d.User.LastName : ""
                })
                .ToListAsync()
                .ConfigureAwait(false);
            timelineNames = timelineRows.ToDictionary(x => x.Id, x => $"{x.FirstName} {x.LastName}".Trim());
        }

        var designerActivityList = timelineAgg
            .Select(x =>
            {
                var name = timelineNames.TryGetValue(x.DesignerId, out var n) && !string.IsNullOrWhiteSpace(n) ? n : "Unknown";
                return new DesignerActivityTimelineItemDto
                {
                    DateKey = x.Date.ToString("yyyy-MM-dd"),
                    Date = x.Date.ToString("MMM d"),
                    DesignerId = x.DesignerId,
                    DesignerName = name,
                    OrdersCompleted = x.OrdersCompleted,
                    FilesUploaded = 0
                };
            })
            .OrderBy(x => x.DateKey)
            .Take(50)
            .ToList();

        var dto = new DesignerAnalyticsDto
        {
            DesignerPerformance = result.OrderByDescending(x => x.OrdersCompleted).ToList(),
            DesignerActivityTimeline = designerActivityList
        };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<ClientAnalyticsDto> GetClientAnalyticsAsync()
    {
        var cacheKey = $"ldp:cache:analytics:clients:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<ClientAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalClients = await _context.ClientProfiles.CountAsync().ConfigureAwait(false);
        var newClients = await _context.LogoOrders.AsNoTracking()
            .GroupBy(o => o.ClientId)
            .Where(g => g.Min(o => o.CreatedAt) >= startOfSixMonths)
            .CountAsync()
            .ConfigureAwait(false);
        var returningClients = Math.Max(0, totalClients - newClients);

        var clientAgg = await _context.LogoOrders.AsNoTracking()
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, OrderCount = g.Count() })
            .OrderByDescending(x => x.OrderCount)
            .Take(15)
            .ToListAsync()
            .ConfigureAwait(false);
        var clientIds = clientAgg.Select(x => x.ClientId).ToList();
        var clientsInfo = await _context.ClientProfiles.AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CompanyName, c.User!.FirstName, c.User.LastName })
            .ToListAsync()
            .ConfigureAwait(false);
        var clientLookup = clientsInfo.ToDictionary(
            d => d.Id,
            d => string.IsNullOrWhiteSpace($"{d.FirstName} {d.LastName}".Trim())
                ? (d.CompanyName ?? "Unknown")
                : $"{d.FirstName} {d.LastName} ({d.CompanyName})");
        var ordersPerClient = clientAgg
            .Select(x => new OrdersPerClientItemDto
            {
                ClientName = clientLookup.TryGetValue(x.ClientId, out var cn) ? cn : "Unknown",
                OrderCount = x.OrderCount
            })
            .ToList();

        var retentionTrend = new List<ClientRetentionTrendItemDto>();
        for (var i = 5; i >= 0; i--)
        {
            var monthStart = startOfSixMonths.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var newInMonth = await _context.LogoOrders.AsNoTracking()
                .GroupBy(o => o.ClientId)
                .Where(g => g.Min(o => o.CreatedAt) >= monthStart && g.Min(o => o.CreatedAt) < monthEnd)
                .CountAsync()
                .ConfigureAwait(false);
            var ordersInMonth = await _context.LogoOrders.AsNoTracking()
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
                .CountAsync()
                .ConfigureAwait(false);
            var returningInMonth = Math.Max(0, ordersInMonth - newInMonth);
            retentionTrend.Add(new ClientRetentionTrendItemDto
            {
                MonthKey = $"{monthStart.Year}-{monthStart.Month:D2}",
                Month = monthStart.ToString("MMM yyyy"),
                NewClients = newInMonth,
                ReturningOrders = returningInMonth
            });
        }

        var topClientsByOrders = clientAgg.Take(10).Select(x => new TopClientByOrdersItemDto
        {
            ClientId = x.ClientId,
            ClientName = clientLookup.TryGetValue(x.ClientId, out var cn) ? cn : "Unknown",
            OrderCount = x.OrderCount
        }).ToList();

        var ltvAgg = await _context.LogoOrders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Revenue = g.Sum(x => x.Price), OrderCount = g.Count() })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync()
            .ConfigureAwait(false);
        var ltvIds = ltvAgg.Select(x => x.ClientId).ToList();
        var ltvClientsInfo = await _context.ClientProfiles.AsNoTracking()
            .Where(c => ltvIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CompanyName, c.User!.FirstName, c.User.LastName })
            .ToListAsync()
            .ConfigureAwait(false);
        var ltvLookup = ltvClientsInfo.ToDictionary(
            d => d.Id,
            d => string.IsNullOrWhiteSpace($"{d.FirstName} {d.LastName}".Trim())
                ? (d.CompanyName ?? "Unknown")
                : $"{d.FirstName} {d.LastName} ({d.CompanyName})");
        var clientLifetimeValue = ltvAgg.Select(x => new ClientLifetimeValueItemDto
        {
            ClientId = x.ClientId,
            ClientName = ltvLookup.TryGetValue(x.ClientId, out var nm) ? nm : "Unknown",
            Revenue = x.Revenue,
            OrderCount = x.OrderCount
        }).ToList();

        var dto = new ClientAnalyticsDto
        {
            NewClients = newClients,
            ReturningClients = returningClients,
            OrdersPerClient = ordersPerClient,
            ClientRetentionTrend = retentionTrend,
            TopClientsByOrders = topClientsByOrders,
            ClientLifetimeValue = clientLifetimeValue
        };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<WorkflowAnalyticsDto> GetWorkflowAnalyticsAsync()
    {
        var cacheKey = $"ldp:cache:analytics:workflow:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<WorkflowAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var q = _context.LogoOrders.AsNoTracking().Where(o => !o.IsDeleted);

        var ordersWithRevisions = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrderId)
            .Distinct()
            .CountAsync();

        var funnel = new OrderFunnelDto
        {
            OrdersCreated = await q.CountAsync(),
            AssignedToDesigner = await q.CountAsync(o => o.DesignerId.HasValue),
            DesignSubmitted = await q.CountAsync(o =>
                o.Status == OrderStatus.PreviewDelivered || o.Status == OrderStatus.RevisionRequested || o.Status == OrderStatus.Completed),
            RevisionRequested = ordersWithRevisions,
            ClientApproval = await q.CountAsync(o =>
                o.Status == OrderStatus.PreviewDelivered || o.Status == OrderStatus.ClientApproved),
            Completed = await q.CountAsync(o => o.Status == OrderStatus.Completed)
        };

        var completedForAvg = q.Where(o => o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= o.CreatedAt);
        double avgCompletion = 0;
        if (await completedForAvg.AnyAsync())
        {
            var deliveryRows = await completedForAvg
                .Select(o => new { o.CreatedAt, End = o.UpdatedAt ?? o.CreatedAt })
                .Where(x => x.End >= x.CreatedAt)
                .ToListAsync();
            if (deliveryRows.Count > 0)
                avgCompletion = deliveryRows.Average(x => (x.End - x.CreatedAt).TotalDays);
        }

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

        var totalOrders = await q.CountAsync();
        var relevantRevisions = revisionCounts;
        var avgRevisions = totalOrders > 0 && relevantRevisions.Count > 0
            ? relevantRevisions.Average(x => (double)x.Count)
            : 0;

        var revCountDict = relevantRevisions.ToDictionary(x => x.OrderId, x => x.Count);
        var ordersWith1Revision = revCountDict.Count(x => x.Value == 1);
        var ordersWith2PlusRevisions = revCountDict.Count(x => x.Value >= 2);
        var ordersWithNoRevisions = Math.Max(0, totalOrders - revCountDict.Count);

        var ordersStuckInStage = new List<OrdersStuckInStageItemDto>
        {
            new() { Stage = "Awaiting Admin Review", Count = await q.CountAsync(o => o.Status == OrderStatus.WaitingForAdminApproval || o.Status == OrderStatus.PriceApprovalPending) },
            new() { Stage = "Unassigned", Count = await q.CountAsync(o => !o.DesignerId.HasValue && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled) },
            new() { Stage = "In Progress", Count = await q.CountAsync(o => o.Status == OrderStatus.InProgress) },
            new() { Stage = "Revision Requested", Count = await q.CountAsync(o => o.Status == OrderStatus.RevisionRequested) },
            new() { Stage = "Awaiting Client Approval", Count = await q.CountAsync(o => o.Status == OrderStatus.PreviewDelivered) }
        }.Where(x => x.Count > 0).ToList();

        var dto = new WorkflowAnalyticsDto
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
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
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
        // Longer TTL than other admin charts: forecast is expensive to aggregate and tolerates slightly staler data.
        var cacheKey = $"ldp:cache:analytics:forecast:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<ForecastAnalyticsDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var orderTrendRaw = await _context.LogoOrders.AsNoTracking()
            .Where(o => o.CreatedAt >= startOfSixMonths)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new { Y = g.Key.Year, M = g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Y).ThenBy(x => x.M)
            .ToListAsync()
            .ConfigureAwait(false);
        var orderTrend = orderTrendRaw.Select(g => new
        {
            Count = g.Count,
            MonthKey = $"{g.Y}-{g.M:D2}",
            Month = new DateTime(g.Y, g.M, 1).ToString("MMM yyyy")
        }).ToList();

        var qCompleted = _context.LogoOrders.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
        var revenueTrendRaw = await qCompleted
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Y = (o.UpdatedAt ?? o.CreatedAt).Year, M = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new { g.Key.Y, g.Key.M, Revenue = g.Sum(x => x.Price) })
            .OrderBy(x => x.Y).ThenBy(x => x.M)
            .ToListAsync()
            .ConfigureAwait(false);
        var revenueTrend = revenueTrendRaw.Select(g => new
        {
            Revenue = g.Revenue,
            MonthKey = $"{g.Y}-{g.M:D2}",
            Month = new DateTime(g.Y, g.M, 1).ToString("MMM yyyy")
        }).ToList();

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

        var dto = new ForecastAnalyticsDto
        {
            OrderGrowthPrediction = orderPrediction,
            RevenueForecast = revenuePrediction
        };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(15), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<InsightsAnalyticsDto> GetInsightsAsync()
    {
        var cacheKey = $"ldp:cache:analytics:insights:e{_cacheVersions.AnalyticsEpoch}";
        var cached = await DistributedJsonCache.GetAsync<InsightsAnalyticsDto>(_distributedCache, cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var dto = await BuildInsightsUncachedAsync().ConfigureAwait(false);
        await DistributedJsonCache.SetAsync(_distributedCache, cacheKey, dto, TimeSpan.FromSeconds(45)).ConfigureAwait(false);
        return dto;
    }

    /// <summary>
    /// Insights without loading full order/designer graphs; uses counts and grouped projections.
    /// </summary>
    private async Task<InsightsAnalyticsDto> BuildInsightsUncachedAsync()
    {
        var insights = new List<string>();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var q = _context.LogoOrders.AsNoTracking().Where(o => !o.IsDeleted);

        var ordersThisMonth = await q.CountAsync(o => o.CreatedAt >= startOfMonth).ConfigureAwait(false);
        var ordersLastMonth = await q.CountAsync(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt < startOfMonth).ConfigureAwait(false);
        if (ordersLastMonth > 0)
        {
            var pctChange = (ordersThisMonth - ordersLastMonth) * 100.0 / ordersLastMonth;
            insights.Add($"Orders {(pctChange >= 0 ? "increased" : "decreased")} {Math.Abs(Math.Round(pctChange, 0))}% this month compared to last month.");
        }

        var totalRevenue = await q.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => o.Price).ConfigureAwait(false);
        var revenueThisMonth = await q
            .Where(o => o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .SumAsync(o => o.Price)
            .ConfigureAwait(false);
        var revenueLastMonth = await q
            .Where(o => o.Status == OrderStatus.Completed
                        && (o.UpdatedAt ?? o.CreatedAt) >= startOfLastMonth
                        && (o.UpdatedAt ?? o.CreatedAt) < startOfMonth)
            .SumAsync(o => o.Price)
            .ConfigureAwait(false);
        if (revenueLastMonth > 0)
        {
            var pctChange = (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100);
            insights.Add($"Revenue {(pctChange >= 0 ? "increased" : "decreased")} {Math.Abs(Math.Round(pctChange, 0))}% this month.");
        }

        var designerAgg = await q
            .Where(o => o.DesignerId.HasValue)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new
            {
                DesignerId = g.Key,
                Total = g.Count(),
                Completed = g.Count(x => x.Status == OrderStatus.Completed)
            })
            .Where(x => x.Total >= 3)
            .ToListAsync()
            .ConfigureAwait(false);

        var topEntry = designerAgg
            .Select(x => new
            {
                x.DesignerId,
                Rate = x.Total > 0 ? (decimal)x.Completed / x.Total * 100 : 0m,
                x.Total
            })
            .OrderByDescending(x => x.Rate)
            .FirstOrDefault();

        if (topEntry != null)
        {
            var topName = await _context.DesignerProfiles
                .AsNoTracking()
                .Where(d => d.Id == topEntry.DesignerId && !d.IsDeleted)
                .Select(d => new { d.User!.FirstName, d.User!.LastName })
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (topName != null)
            {
                insights.Add($"{topName.FirstName} {topName.LastName} has the highest approval rate ({Math.Round(topEntry.Rate, 0)}%) among designers with 3+ orders.");
            }
        }

        var ordersWithRevisions = await _context.OrderRevisions
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrderId)
            .Distinct()
            .CountAsync()
            .ConfigureAwait(false);
        var totalOrders = await q.CountAsync().ConfigureAwait(false);
        var revisionPct = totalOrders > 0 ? (decimal)ordersWithRevisions / totalOrders * 100 : 0;
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
