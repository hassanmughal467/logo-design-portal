using LogoDesignPortal.Application.DTOs.Client;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class ClientFinancialInsightsService : IClientFinancialInsightsService
{
    private readonly IApplicationDbContext _context;

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

    public ClientFinancialInsightsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialInsightsResponseDto> GetFinancialInsightsAsync(Guid clientUserId)
    {
        var insights = new List<FinancialInsightDto>();

        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientUserId && !c.IsDeleted);
        if (client == null)
        {
            return new FinancialInsightsResponseDto { Insights = insights };
        }

        var orders = await _context.LogoOrders
            .Where(o => o.ClientId == client.Id && !o.IsDeleted && !o.IsArchived)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var endOfLastMonth = startOfThisMonth.AddTicks(-1);
        var thirtyDaysAgo = now.AddDays(-30);

        var ordersThisMonth = orders.Where(o => o.CreatedAt >= startOfThisMonth).ToList();
        var ordersLastMonth = orders.Where(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt <= endOfLastMonth).ToList();
        var lastOrderDate = orders.FirstOrDefault()?.CreatedAt;
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var nonCancelledOrders = orders.Where(o =>
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.CancelledByUser &&
            o.Status != OrderStatus.CancelledByAdmin).ToList();

        // 1. Orders placed this month
        var countThisMonth = ordersThisMonth.Count;
        insights.Add(new FinancialInsightDto
        {
            Message = countThisMonth == 0
                ? "No orders placed this month yet."
                : countThisMonth == 1
                    ? "1 order placed this month."
                    : $"{countThisMonth} orders placed this month.",
            Severity = "info",
            Icon = "pi pi-shopping-cart"
        });

        // 2. Spending increase or decrease compared to last month (completed orders only)
        var spendThisMonth = completedOrders.Where(o => o.CreatedAt >= startOfThisMonth).Sum(o => o.Price);
        var spendLastMonth = completedOrders.Where(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt <= endOfLastMonth).Sum(o => o.Price);
        if (spendLastMonth > 0)
        {
            var pctChange = ((spendThisMonth - spendLastMonth) / spendLastMonth) * 100;
            var direction = pctChange >= 0 ? "increase" : "decrease";
            var absPct = Math.Abs(Math.Round(pctChange, 1));
            insights.Add(new FinancialInsightDto
            {
                Message = $"Spending {direction} of {absPct}% compared to last month.",
                Severity = pctChange >= 0 ? "success" : "warning",
                Icon = pctChange >= 0 ? "pi pi-arrow-up" : "pi pi-arrow-down"
            });
        }
        else if (spendThisMonth > 0)
        {
            insights.Add(new FinancialInsightDto
            {
                Message = "New spending this month (no orders last month).",
                Severity = "success",
                Icon = "pi pi-chart-line"
            });
        }

        // 3. Most used package (from completed orders)
        if (completedOrders.Count > 0)
        {
            var byPackage = completedOrders
                .GroupBy(o => GetPackageFromPrice(o.Price))
                .OrderByDescending(g => g.Count())
                .First();
            insights.Add(new FinancialInsightDto
            {
                Message = $"Most used package: {byPackage.Key} ({byPackage.Count()} order{(byPackage.Count() == 1 ? "" : "s")}).",
                Severity = "info",
                Icon = "pi pi-box"
            });
        }

        // 4. Average order value (Lifetime Spend / Completed Orders)
        var lifetimeSpend = completedOrders.Sum(o => o.Price);
        if (completedOrders.Count > 0 && lifetimeSpend > 0)
        {
            var avgOrderValue = lifetimeSpend / completedOrders.Count;
            insights.Add(new FinancialInsightDto
            {
                Message = $"Average order value: ${avgOrderValue:N2}.",
                Severity = "info",
                Icon = "pi pi-percentage"
            });
        }

        // 5. Optional: Client inactivity warning if no orders in 30 days
        if (lastOrderDate.HasValue && lastOrderDate.Value < thirtyDaysAgo)
        {
            var daysSince = (int)(now - lastOrderDate.Value).TotalDays;
            insights.Add(new FinancialInsightDto
            {
                Message = $"No orders in the last {daysSince} days. Consider reaching out for your next project.",
                Severity = "warning",
                Icon = "pi pi-exclamation-triangle"
            });
        }
        else if (orders.Count == 0)
        {
            insights.Add(new FinancialInsightDto
            {
                Message = "No orders yet. Start your first logo project when you're ready!",
                Severity = "info",
                Icon = "pi pi-info-circle"
            });
        }

        return new FinancialInsightsResponseDto { Insights = insights };
    }
}
