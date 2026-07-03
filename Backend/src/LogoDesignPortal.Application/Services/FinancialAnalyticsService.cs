using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

public class FinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _distributedCache;
    private readonly IReadModelCacheVersions _readModelCacheVersions;
    private readonly ILogger<FinancialAnalyticsService> _logger;

    public FinancialAnalyticsService(
        IApplicationDbContext context,
        IDistributedCache distributedCache,
        IReadModelCacheVersions readModelCacheVersions,
        ILogger<FinancialAnalyticsService> logger)
    {
        _context = context;
        _distributedCache = distributedCache;
        _readModelCacheVersions = readModelCacheVersions;
        _logger = logger;
    }

    private static string GetPackageFromPrice(decimal price)
    {
        if (price < 200)
        {
            return "Basic";
        }

        if (price < 500)
        {
            return "Standard";
        }

        if (price < 1000)
        {
            return "Premium";
        }

        return "Custom";
    }

    public async Task<FinancialOverviewDto> GetOverviewAsync()
    {
        var cacheKey = $"ldp:cache:financial:overview:e{_readModelCacheVersions.OrdersEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<FinancialOverviewDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        var dto = await BuildFinancialOverviewUncachedAsync().ConfigureAwait(false);
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(7), _logger).ConfigureAwait(false);
        return dto;
    }

    private async Task<FinancialOverviewDto> BuildFinancialOverviewUncachedAsync()
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var comp = _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed);

        var totalRevenue = await comp.SumAsync(o => o.Price);
        var refundedAmount = await comp
            .Where(o => o.IsRefunded && o.RefundAmount.HasValue)
            .SumAsync(o => o.RefundAmount ?? 0);

        var inv = _context.Invoices.AsNoTracking().Where(i => !i.IsDeleted);
        var collectedRevenue = await inv.Where(i => i.Status == InvoiceStatus.Paid).SumAsync(i => i.TotalAmount);
        var outstandingBalance = await inv
            .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
            .SumAsync(i => i.TotalAmount);

        var revenueThisMonth = await comp
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .SumAsync(o => o.Price);
        var revenueThisWeek = await comp
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfWeek)
            .SumAsync(o => o.Price);
        var lastMonthRevenue = await comp
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfLastMonth && (o.UpdatedAt ?? o.CreatedAt) < startOfMonth)
            .SumAsync(o => o.Price);

        var revenueGrowth = lastMonthRevenue > 0
            ? (decimal)((revenueThisMonth - lastMonthRevenue) / lastMonthRevenue * 100)
            : 0;

        var totalOrders = await _context.LogoOrders.AsNoTracking().CountAsync(o => !o.IsDeleted);
        var completedCount = await comp.CountAsync();
        var avgOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0;

        var perCurrencyRaw = await comp
            .GroupBy(o => o.CurrencyCode == null || o.CurrencyCode == "" ? "USD" : o.CurrencyCode)
            .Select(g => new
            {
                CurrencyCode = g.Key,
                TotalRevenue = g.Sum(o => o.Price),
                MonthlyRevenue = g.Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth).Sum(o => o.Price),
                CompletedCount = g.Count()
            })
            .ToListAsync();

        var revenueByCurrency = perCurrencyRaw
            .Select(g => new RevenueByCurrencyItemDto
            {
                CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(g.CurrencyCode),
                TotalRevenue = g.TotalRevenue,
                MonthlyRevenue = g.MonthlyRevenue,
                CompletedCount = g.CompletedCount,
                AverageOrderValue = g.CompletedCount > 0 ? g.TotalRevenue / g.CompletedCount : 0
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

        var (revenueCurrencyCode, revenueCurrencyMixed) = revenueByCurrency.Count switch
        {
            1 => (revenueByCurrency[0].CurrencyCode, false),
            > 1 => (ClientCurrencyHelper.DefaultCode, true),
            _ => (ClientCurrencyHelper.DefaultCode, false)
        };

        return new FinancialOverviewDto
        {
            TotalRevenue = totalRevenue,
            RevenueCurrencyCode = revenueCurrencyCode,
            RevenueCurrencyMixed = revenueCurrencyMixed,
            RevenueByCurrency = revenueByCurrency,
            RevenueThisMonth = revenueThisMonth,
            RevenueThisWeek = revenueThisWeek,
            AverageOrderValue = avgOrderValue,
            TotalOrders = totalOrders,
            RevenueGrowthPercent = Math.Round(revenueGrowth, 1),
            InvoicesPaid = await inv.CountAsync(i => i.Status == InvoiceStatus.Paid),
            InvoicesPending = await inv.CountAsync(i => i.Status == InvoiceStatus.Pending),
            OverdueInvoices = await inv.CountAsync(i => i.Status == InvoiceStatus.Overdue),
            OutstandingBalance = outstandingBalance,
            CollectedRevenue = collectedRevenue,
            RefundedAmount = refundedAmount
        };
    }

    public async Task<RevenueTrendDto> GetRevenueTrendAsync()
    {
        var cacheKey = $"ldp:cache:financial:revenueTrend:e{_readModelCacheVersions.OrdersEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<RevenueTrendDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var raw = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(x => x.Price)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var items = raw.Select(x => new FinancialRevenueTrendItemDto
        {
            MonthKey = $"{x.Year}-{x.Month:D2}",
            Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
            Revenue = x.Revenue
        }).ToList();

        var dto = new RevenueTrendDto { Items = items };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(8), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<OrdersVsRevenueDto> GetOrdersVsRevenueAsync()
    {
        var cacheKey = $"ldp:cache:financial:ordersVsRevenue:e{_readModelCacheVersions.OrdersEpoch}";
        var cached = await DistributedJsonCache.GetSafeAsync<OrdersVsRevenueDto>(_distributedCache, cacheKey, _logger).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var byMonth = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.CreatedAt >= startOfSixMonths)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                OrdersCount = g.Count(),
                Revenue = g.Sum(x => x.Status == OrderStatus.Completed ? x.Price : 0m)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var items = byMonth.Select(x => new OrdersVsRevenueItemDto
        {
            MonthKey = $"{x.Year}-{x.Month:D2}",
            Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
            OrdersCount = x.OrdersCount,
            Revenue = x.Revenue
        }).ToList();

        var dto = new OrdersVsRevenueDto { Items = items };
        await DistributedJsonCache.SetSafeAsync(_distributedCache, cacheKey, dto, TimeSpan.FromMinutes(8), _logger).ConfigureAwait(false);
        return dto;
    }

    public async Task<PackageRevenueDto> GetPackageRevenueAsync()
    {
        var q = _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed);

        var basic = await q.Where(o => o.Price < 200).SumAsync(o => o.Price);
        var standard = await q.Where(o => o.Price >= 200 && o.Price < 500).SumAsync(o => o.Price);
        var premium = await q.Where(o => o.Price >= 500 && o.Price < 1000).SumAsync(o => o.Price);
        var custom = await q.Where(o => o.Price >= 1000).SumAsync(o => o.Price);

        var items = new[]
            {
                new PackageRevenueItemDto { Package = "Basic", Revenue = basic },
                new PackageRevenueItemDto { Package = "Standard", Revenue = standard },
                new PackageRevenueItemDto { Package = "Premium", Revenue = premium },
                new PackageRevenueItemDto { Package = "Custom", Revenue = custom }
            }
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return new PackageRevenueDto { Items = items };
    }

    public async Task<DesignerRevenueDto> GetDesignerRevenueAsync()
    {
        var agg = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new
            {
                DesignerId = g.Key,
                TotalRevenue = g.Sum(o => o.Price),
                OrdersCompleted = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(15)
            .ToListAsync();

        var ids = agg.Select(x => x.DesignerId).ToList();
        var names = await _context.DesignerProfiles.AsNoTracking()
            .Where(d => ids.Contains(d.Id) && !d.IsDeleted)
            .Select(d => new { d.Id, d.User!.FirstName, d.User.LastName })
            .ToListAsync();
        var nameLookup = names.ToDictionary(x => x.Id, x => $"{x.FirstName} {x.LastName}".Trim());

        var items = agg.Select(x => new DesignerRevenueItemDto
        {
            DesignerId = x.DesignerId,
            DesignerName = nameLookup.TryGetValue(x.DesignerId, out var n) && !string.IsNullOrWhiteSpace(n) ? n : "Unknown",
            TotalRevenue = x.TotalRevenue,
            OrdersCompleted = x.OrdersCompleted
        }).ToList();

        return new DesignerRevenueDto { Items = items };
    }

    public async Task<ClientRevenueDto> GetClientRevenueAsync()
    {
        var agg = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                OrdersCount = g.Count(),
                TotalRevenue = g.Sum(o => o.Price)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(15)
            .ToListAsync();

        var ids = agg.Select(x => x.ClientId).ToList();
        var profiles = await _context.ClientProfiles.AsNoTracking()
            .Where(c => ids.Contains(c.Id) && !c.IsDeleted)
            .Select(c => new { c.Id, c.CompanyName, c.CurrencyCode, c.User!.FirstName, c.User.LastName })
            .ToListAsync();
        var profileLookup = profiles.ToDictionary(x => x.Id);

        var items = agg.Select(x =>
        {
            profileLookup.TryGetValue(x.ClientId, out var p);
            var name = p == null ? "Unknown"
                : string.IsNullOrWhiteSpace($"{p.FirstName} {p.LastName}".Trim())
                    ? (p.CompanyName ?? "Unknown")
                    : $"{p.FirstName} {p.LastName} ({p.CompanyName})";
            return new ClientRevenueItemDto
            {
                ClientId = x.ClientId,
                ClientName = name,
                OrdersCount = x.OrdersCount,
                TotalRevenue = x.TotalRevenue,
                CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(p?.CurrencyCode)
            };
        }).ToList();

        return new ClientRevenueDto { Items = items };
    }

    public async Task<InvoiceStatusDto> GetInvoiceStatusAsync()
    {
        var counts = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var statusMap = new Dictionary<string, string>
        {
            [InvoiceStatus.Pending.ToString()] = "Pending",
            [InvoiceStatus.Paid.ToString()] = "Paid",
            [InvoiceStatus.Due.ToString()] = "Due",
            [InvoiceStatus.Overdue.ToString()] = "Overdue"
        };

        var items = counts.Select(c => new InvoiceStatusItemDto
        {
            Status = statusMap.GetValueOrDefault(c.Status, c.Status),
            Count = c.Count
        }).ToList();

        return new InvoiceStatusDto { Items = items };
    }

    public async Task<WeeklyRevenueDto> GetWeeklyRevenueAsync()
    {
        var last7Days = DateTime.UtcNow.AddDays(-7);
        var startOfPeriod = new DateTime(last7Days.Year, last7Days.Month, last7Days.Day, 0, 0, 0, DateTimeKind.Utc);

        var byDateList = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfPeriod)
            .GroupBy(o => (o.UpdatedAt ?? o.CreatedAt).Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Price) })
            .ToListAsync();
        var byDate = byDateList.ToDictionary(x => x.Date, x => x.Revenue);

        var items = new List<WeeklyRevenueItemDto>();
        for (var i = 0; i < 7; i++)
        {
            var d = DateTime.UtcNow.Date.AddDays(-6 + i);
            items.Add(new WeeklyRevenueItemDto
            {
                DayIndex = i,
                DayOfWeek = d.ToString("ddd M/d"),
                Revenue = byDate.TryGetValue(d, out var rev) ? rev : 0
            });
        }

        return new WeeklyRevenueDto { Items = items };
    }

    public async Task<OrderValueTrendDto> GetOrderValueTrendAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var raw = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                AverageOrderValue = g.Average(x => x.Price),
                Count = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var items = raw.Select(x => new OrderValueTrendItemDto
        {
            MonthKey = $"{x.Year}-{x.Month:D2}",
            Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
            AverageOrderValue = x.Count > 0 ? x.AverageOrderValue : 0
        }).ToList();

        return new OrderValueTrendDto { Items = items };
    }

    public async Task<FinancialActivityFeedDto> GetActivityFeedAsync()
    {
        var last30Days = DateTime.UtcNow.AddDays(-30);
        var items = new List<FinancialActivityItemDto>();

        var paidInvoiceRows = await _context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid && i.PaidDate.HasValue && i.PaidDate >= last30Days)
            .OrderByDescending(i => i.PaidDate)
            .Take(10)
            .Select(i => new { i.InvoiceNumber, i.TotalAmount, i.PaidDate, i.ClientId })
            .ToListAsync();
        var paidClientIds = paidInvoiceRows.Select(i => i.ClientId).Distinct().ToList();
        var paidClientCurrencies = await _context.ClientProfiles.AsNoTracking()
            .Where(c => paidClientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.CurrencyCode);
        items.AddRange(paidInvoiceRows.Select(i => new FinancialActivityItemDto
        {
            Type = "InvoicePaid",
            Description = $"Invoice {i.InvoiceNumber} paid",
            Amount = i.TotalAmount,
            OccurredAt = i.PaidDate!.Value,
            CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(
                paidClientCurrencies.GetValueOrDefault(i.ClientId))
        }));

        var newOrderRows = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.CreatedAt >= last30Days)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new { o.Title, o.Price, o.CurrencyCode, o.CreatedAt })
            .ToListAsync();
        items.AddRange(newOrderRows.Select(o => new FinancialActivityItemDto
        {
            Type = "NewOrder",
            Description = $"New order: {o.Title}",
            Amount = o.Price,
            OccurredAt = o.CreatedAt,
            CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(o.CurrencyCode)
        }));

        var completedOrderRows = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && o.UpdatedAt.HasValue && o.UpdatedAt >= last30Days)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(10)
            .Select(o => new { o.Title, o.Price, o.CurrencyCode, UpdatedAt = o.UpdatedAt!.Value })
            .ToListAsync();
        items.AddRange(completedOrderRows.Select(o => new FinancialActivityItemDto
        {
            Type = "OrderCompleted",
            Description = $"Order completed: {o.Title}",
            Amount = o.Price,
            OccurredAt = o.UpdatedAt,
            CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(o.CurrencyCode)
        }));

        var refundRows = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.IsRefunded && o.RefundedAt.HasValue && o.RefundedAt >= last30Days)
            .OrderByDescending(o => o.RefundedAt)
            .Take(10)
            .Select(o => new { o.Title, RefundAmount = o.RefundAmount ?? 0, o.CurrencyCode, RefundedAt = o.RefundedAt!.Value })
            .ToListAsync();
        items.AddRange(refundRows.Select(o => new FinancialActivityItemDto
        {
            Type = "RefundIssued",
            Description = $"Refund: {o.Title}",
            Amount = o.RefundAmount,
            OccurredAt = o.RefundedAt,
            CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(o.CurrencyCode)
        }));

        var largeThreshold = 1000m;
        var largePaymentRows = await _context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid && i.PaidDate.HasValue && i.PaidDate >= last30Days && i.TotalAmount >= largeThreshold)
            .OrderByDescending(i => i.PaidDate)
            .Take(5)
            .Select(i => new { i.InvoiceNumber, i.TotalAmount, i.PaidDate, i.ClientId })
            .ToListAsync();
        var largeClientIds = largePaymentRows.Select(i => i.ClientId).Distinct().ToList();
        var largeClientCurrencies = largeClientIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _context.ClientProfiles.AsNoTracking()
                .Where(c => largeClientIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CurrencyCode);
        items.AddRange(largePaymentRows.Select(i => new FinancialActivityItemDto
        {
            Type = "LargeTransaction",
            Description = $"Large payment: {i.InvoiceNumber}",
            Amount = i.TotalAmount,
            OccurredAt = i.PaidDate!.Value,
            CurrencyCode = ClientCurrencyHelper.NormalizeOrDefault(
                largeClientCurrencies.GetValueOrDefault(i.ClientId))
        }));

        var sorted = items.OrderByDescending(x => x.OccurredAt).Take(25).ToList();
        return new FinancialActivityFeedDto { Items = sorted };
    }

    public async Task<RevenueForecastDto> GetRevenueForecastAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthly = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Price) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var revenueValues = monthly.Select(x => (double)x.Revenue).ToList();
        var items = new List<FinancialRevenueForecastItemDto>();

        for (var i = 0; i < monthly.Count; i++)
        {
            var m = monthly[i];
            decimal? predicted = null;
            if (i >= 2)
            {
                var window = revenueValues.Skip(i - 2).Take(2).ToList();
                predicted = (decimal)Math.Round(window.Average(), 2);
            }
            items.Add(new FinancialRevenueForecastItemDto
            {
                MonthKey = $"{m.Year}-{m.Month:D2}",
                Month = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                Actual = m.Revenue,
                Predicted = predicted
            });
        }

        if (revenueValues.Count >= 2)
        {
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            items.Add(new FinancialRevenueForecastItemDto
            {
                MonthKey = $"{nextMonth.Year}-{nextMonth.Month:D2}",
                Month = nextMonth.ToString("MMM yyyy"),
                Actual = null,
                Predicted = (decimal)Math.Round(revenueValues.TakeLast(2).Average(), 2)
            });
        }

        return new RevenueForecastDto { Items = items };
    }
}
