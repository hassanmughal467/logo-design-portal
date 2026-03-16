using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class FinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly IApplicationDbContext _context;

    public FinancialAnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    private static string GetPackageFromPrice(decimal price)
    {
        if (price < 200) return "Basic";
        if (price < 500) return "Standard";
        if (price < 1000) return "Premium";
        return "Custom";
    }

    public async Task<FinancialOverviewDto> GetOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.RefundAmount, o.IsRefunded, o.UpdatedAt, o.CreatedAt })
            .ToListAsync();

        var invoices = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .Select(i => new { i.TotalAmount, i.Status, i.PaidDate })
            .ToListAsync();

        var totalRevenue = completedOrders.Sum(o => o.Price);
        var refundedAmount = completedOrders.Where(o => o.IsRefunded && o.RefundAmount.HasValue).Sum(o => o.RefundAmount ?? 0);
        var collectedRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
        var outstandingBalance = invoices.Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount);

        var revenueThisMonth = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfMonth)
            .Sum(o => o.Price);

        var revenueThisWeek = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfWeek)
            .Sum(o => o.Price);

        var lastMonthRevenue = completedOrders
            .Where(o =>
            {
                var d = o.UpdatedAt ?? o.CreatedAt;
                return d >= startOfLastMonth && d < startOfMonth;
            })
            .Sum(o => o.Price);

        var revenueGrowth = lastMonthRevenue > 0
            ? (decimal)((revenueThisMonth - lastMonthRevenue) / lastMonthRevenue * 100)
            : 0;

        var totalOrders = await _context.LogoOrders.CountAsync(o => !o.IsDeleted);
        var completedCount = completedOrders.Count;
        var avgOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0;

        return new FinancialOverviewDto
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueThisWeek = revenueThisWeek,
            AverageOrderValue = avgOrderValue,
            TotalOrders = totalOrders,
            RevenueGrowthPercent = Math.Round(revenueGrowth, 1),
            InvoicesPaid = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            InvoicesPending = invoices.Count(i => i.Status == InvoiceStatus.Pending),
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            OutstandingBalance = outstandingBalance,
            CollectedRevenue = collectedRevenue,
            RefundedAmount = refundedAmount
        };
    }

    public async Task<RevenueTrendDto> GetRevenueTrendAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.UpdatedAt, o.CreatedAt })
            .ToListAsync();

        var items = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new FinancialRevenueTrendItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Revenue = g.Sum(x => x.Price)
            })
            .ToList();

        return new RevenueTrendDto { Items = items };
    }

    public async Task<OrdersVsRevenueDto> GetOrdersVsRevenueAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.Price, o.Status, o.CreatedAt, o.UpdatedAt })
            .ToListAsync();

        var byMonth = orders
            .GroupBy(o => new { Year = o.CreatedAt.Year, Month = o.CreatedAt.Month })
            .Where(g => new DateTime(g.Key.Year, g.Key.Month, 1) >= startOfSixMonths)
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                OrdersCount = g.Count(),
                Revenue = g.Where(x => x.Status == OrderStatus.Completed).Sum(x => x.Price)
            })
            .ToList();

        var items = byMonth.Select(x => new OrdersVsRevenueItemDto
        {
            MonthKey = $"{x.Year}-{x.Month:D2}",
            Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
            OrdersCount = x.OrdersCount,
            Revenue = x.Revenue
        }).ToList();

        return new OrdersVsRevenueDto { Items = items };
    }

    public async Task<PackageRevenueDto> GetPackageRevenueAsync()
    {
        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => o.Price)
            .ToListAsync();

        var items = completedOrders
            .GroupBy(GetPackageFromPrice)
            .Select(g => new PackageRevenueItemDto { Package = g.Key, Revenue = g.Sum(x => x) })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return new PackageRevenueDto { Items = items };
    }

    public async Task<DesignerRevenueDto> GetDesignerRevenueAsync()
    {
        var data = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.DesignerId.HasValue && o.Status == OrderStatus.Completed)
            .Include(o => o.Designer)
            .ThenInclude(d => d!.User)
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g => new
            {
                DesignerId = g.Key,
                DesignerName = g.First().Designer != null && g.First().Designer!.User != null
                    ? $"{g.First().Designer!.User!.FirstName} {g.First().Designer!.User!.LastName}"
                    : "Unknown",
                TotalRevenue = g.Sum(o => o.Price),
                OrdersCompleted = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(15)
            .ToListAsync();

        var items = data.Select(x => new DesignerRevenueItemDto
        {
            DesignerId = x.DesignerId,
            DesignerName = x.DesignerName,
            TotalRevenue = x.TotalRevenue,
            OrdersCompleted = x.OrdersCompleted
        }).ToList();

        return new DesignerRevenueDto { Items = items };
    }

    public async Task<ClientRevenueDto> GetClientRevenueAsync()
    {
        var data = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Include(o => o.Client)
            .ThenInclude(c => c!.User)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                ClientName = g.First().Client != null
                    ? (g.First().Client!.User != null
                        ? $"{g.First().Client!.User!.FirstName} {g.First().Client!.User!.LastName} ({g.First().Client!.CompanyName})"
                        : g.First().Client!.CompanyName)
                    : "Unknown",
                OrdersCount = g.Count(),
                TotalRevenue = g.Sum(o => o.Price)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(15)
            .ToListAsync();

        var items = data.Select(x => new ClientRevenueItemDto
        {
            ClientId = x.ClientId,
            ClientName = x.ClientName,
            OrdersCount = x.OrdersCount,
            TotalRevenue = x.TotalRevenue
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

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfPeriod)
            .Select(o => new { o.Price, Date = (o.UpdatedAt ?? o.CreatedAt).Date })
            .ToListAsync();

        var byDate = completedOrders
            .GroupBy(o => o.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

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

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.UpdatedAt, o.CreatedAt })
            .ToListAsync();

        var items = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new OrderValueTrendItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                AverageOrderValue = g.Count() > 0 ? g.Average(x => x.Price) : 0
            })
            .ToList();

        return new OrderValueTrendDto { Items = items };
    }

    public async Task<FinancialActivityFeedDto> GetActivityFeedAsync()
    {
        var last30Days = DateTime.UtcNow.AddDays(-30);
        var items = new List<FinancialActivityItemDto>();

        var paidInvoices = await _context.Invoices
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid && i.PaidDate.HasValue && i.PaidDate >= last30Days)
            .OrderByDescending(i => i.PaidDate)
            .Take(10)
            .Select(i => new FinancialActivityItemDto
            {
                Type = "InvoicePaid",
                Description = $"Invoice {i.InvoiceNumber} paid",
                Amount = i.TotalAmount,
                OccurredAt = i.PaidDate!.Value
            })
            .ToListAsync();
        items.AddRange(paidInvoices);

        var newOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.CreatedAt >= last30Days)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new FinancialActivityItemDto
            {
                Type = "NewOrder",
                Description = $"New order: {o.Title}",
                Amount = o.Price,
                OccurredAt = o.CreatedAt
            })
            .ToListAsync();
        items.AddRange(newOrders);

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && o.UpdatedAt.HasValue && o.UpdatedAt >= last30Days)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(10)
            .Select(o => new FinancialActivityItemDto
            {
                Type = "OrderCompleted",
                Description = $"Order completed: {o.Title}",
                Amount = o.Price,
                OccurredAt = o.UpdatedAt!.Value
            })
            .ToListAsync();
        items.AddRange(completedOrders);

        var refunds = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.IsRefunded && o.RefundedAt.HasValue && o.RefundedAt >= last30Days)
            .OrderByDescending(o => o.RefundedAt)
            .Take(10)
            .Select(o => new FinancialActivityItemDto
            {
                Type = "RefundIssued",
                Description = $"Refund: {o.Title}",
                Amount = o.RefundAmount ?? 0,
                OccurredAt = o.RefundedAt!.Value
            })
            .ToListAsync();
        items.AddRange(refunds);

        var largeThreshold = 1000m;
        var largePayments = await _context.Invoices
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid && i.PaidDate.HasValue && i.PaidDate >= last30Days && i.TotalAmount >= largeThreshold)
            .OrderByDescending(i => i.PaidDate)
            .Take(5)
            .Select(i => new FinancialActivityItemDto
            {
                Type = "LargeTransaction",
                Description = $"Large payment: {i.InvoiceNumber}",
                Amount = i.TotalAmount,
                OccurredAt = i.PaidDate!.Value
            })
            .ToListAsync();
        items.AddRange(largePayments);

        var sorted = items.OrderByDescending(x => x.OccurredAt).Take(25).ToList();
        return new FinancialActivityFeedDto { Items = sorted };
    }

    public async Task<RevenueForecastDto> GetRevenueForecastAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var startOfSixMonths = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completedOrders = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.UpdatedAt, o.CreatedAt })
            .ToListAsync();

        var monthly = completedOrders
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= startOfSixMonths)
            .GroupBy(o => new { Year = (o.UpdatedAt ?? o.CreatedAt).Year, Month = (o.UpdatedAt ?? o.CreatedAt).Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Price) })
            .ToList();

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
