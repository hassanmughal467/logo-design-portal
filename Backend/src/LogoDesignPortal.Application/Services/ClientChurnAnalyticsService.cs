using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class ClientChurnAnalyticsService : IClientChurnAnalyticsService
{
    private readonly IApplicationDbContext _context;

    private const int InactiveDaysThreshold = 30;
    private const int HealthyMaxScore = 25;
    private const int WarningMaxScore = 50;
    private const int HighRiskMaxScore = 75;

    public ClientChurnAnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    private static string GetClientDisplayName(ClientProfile? client)
    {
        if (client == null)
        {
            return "Unknown";
        }

        var name = client.User != null
            ? $"{client.User.FirstName} {client.User.LastName}".Trim()
            : string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = client.CompanyName ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(client.CompanyName) && name != client.CompanyName)
        {
            return $"{name} ({client.CompanyName})";
        }

        return name;
    }

    private static string GetRiskLevel(int score)
    {
        if (score <= HealthyMaxScore)
        {
            return "Healthy";
        }

        if (score <= WarningMaxScore)
        {
            return "Warning";
        }

        if (score <= HighRiskMaxScore)
        {
            return "HighRisk";
        }

        return "ChurnLikely";
    }

    /// <summary>
    /// Calculates churn risk score (0-100) using:
    /// - Days since last order (higher = more risk)
    /// - Order frequency (lower = more risk)
    /// - Revenue contribution (higher = more urgent if at risk)
    /// </summary>
    public async Task<ClientChurnRiskScoresDto> GetClientRiskScoresAsync()
    {
        var now = DateTime.UtcNow;
        var totalRevenue = await _context.LogoOrders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .SumAsync(o => o.Price);

        if (totalRevenue <= 0)
        {
            totalRevenue = 1;
        }

        var clientDataRaw = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt),
                OrderCount = g.Count(),
                TotalRevenue = g.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Price)
            })
            .ToListAsync();

        var clientIds = clientDataRaw.Select(x => x.ClientId).ToList();
        var clients = await _context.ClientProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(clientIds);

        var items = new List<ClientRiskScoreItemDto>();
        foreach (var raw in clientDataRaw)
        {
            var c = new
            {
                raw.ClientId,
                Client = clients.GetValueOrDefault(raw.ClientId),
                raw.LastOrder,
                raw.OrderCount,
                raw.TotalRevenue
            };
            orderCurrenciesByClient.TryGetValue(c.ClientId, out var orderCodes);
            var daysSince = (int)(now - c.LastOrder).TotalDays;
            var revenuePercent = totalRevenue > 0 ? (c.TotalRevenue / totalRevenue) * 100 : 0;

            // Component scores (0-100 each, higher = more risk)
            var daysScore = Math.Min(100, (daysSince * 100) / 90); // 90 days = max
            var frequencyScore = c.OrderCount <= 1 ? 80 : c.OrderCount <= 3 ? 50 : c.OrderCount <= 6 ? 25 : 0;
            var revenueWeight = Math.Min(1.5m, 0.5m + (revenuePercent / 100)); // High-revenue clients get amplified risk

            var rawScore = (int)((daysScore * 0.5 + frequencyScore * 0.3) * (double)revenueWeight + (daysSince >= InactiveDaysThreshold ? 20 : 0));
            var riskScore = Math.Min(100, Math.Max(0, rawScore));

            items.Add(new ClientRiskScoreItemDto
            {
                ClientId = c.ClientId,
                ClientName = GetClientDisplayName(c.Client),
                RiskScore = riskScore,
                RiskLevel = GetRiskLevel(riskScore),
                DaysSinceLastOrder = daysSince,
                OrderCount = c.OrderCount,
                TotalRevenue = c.TotalRevenue,
                RevenueContributionPercent = Math.Round(revenuePercent, 2),
                LastOrderDate = c.LastOrder,
                CurrencyCode = ClientCurrencyHelper.ResolveClientRevenueCurrency(c.Client?.CurrencyCode, orderCodes)
            });
        }

        items = items.OrderByDescending(x => x.RiskScore).ToList();
        return new ClientChurnRiskScoresDto { Items = items };
    }

    public async Task<ClientChurnAlertsDto> GetClientChurnAlertsAsync(int inactiveDaysThreshold = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-inactiveDaysThreshold);

        var lastOrderByClient = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt),
                OrderCount = g.Count(),
                TotalRevenue = g.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Price)
            })
            .Where(x => x.LastOrder < cutoff)
            .ToListAsync();

        var clientIds = lastOrderByClient.Select(x => x.ClientId).ToList();
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(clientIds);

        var now = DateTime.UtcNow;
        var items = lastOrderByClient
            .OrderByDescending(x => x.TotalRevenue)
            .Select(x =>
            {
                var daysSince = (int)(now - x.LastOrder).TotalDays;
                var client = clients.GetValueOrDefault(x.ClientId);
                orderCurrenciesByClient.TryGetValue(x.ClientId, out var orderCodes);
                var currency = ClientCurrencyHelper.ResolveClientRevenueCurrency(client?.CurrencyCode, orderCodes);
                return new ClientChurnAlertItemDto
                {
                    ClientId = x.ClientId,
                    ClientName = GetClientDisplayName(client),
                    DaysSinceLastOrder = daysSince,
                    LastOrderDate = x.LastOrder,
                    TotalRevenue = x.TotalRevenue,
                    OrderCount = x.OrderCount,
                    AlertMessage = $"Client has been inactive for {daysSince} days. Last order: {x.LastOrder:MMM d, yyyy}. Total revenue: {currency} {x.TotalRevenue:N0}.",
                    CurrencyCode = currency
                };
            })
            .ToList();

        return new ClientChurnAlertsDto { Items = items };
    }

    public async Task<ClientRetentionStatsDto> GetClientRetentionStatsAsync(int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddMonths(-months);
        var startOfPeriod = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var riskScores = await GetClientRiskScoresAsync();
        var totalWithOrders = riskScores.Items.Count;

        var healthy = riskScores.Items.Count(x => x.RiskLevel == "Healthy");
        var warning = riskScores.Items.Count(x => x.RiskLevel == "Warning");
        var highRisk = riskScores.Items.Count(x => x.RiskLevel == "HighRisk");
        var churnLikely = riskScores.Items.Count(x => x.RiskLevel == "ChurnLikely");

        var ordersByMonth = await _context.LogoOrders
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.ClientId, Date = (o.UpdatedAt ?? o.CreatedAt) })
            .Where(o => o.Date >= startOfPeriod)
            .ToListAsync();

        var monthsList = new List<DateTime>();
        for (var d = startOfPeriod; d <= now; d = d.AddMonths(1))
        {
            monthsList.Add(d);
        }

        var retentionTrend = new List<ChurnRetentionTrendItemDto>();
        var inactiveCutoff = 30;

        foreach (var monthStart in monthsList)
        {
            var monthEnd = monthStart.AddMonths(1);
            var activeInMonth = ordersByMonth
                .Where(o => o.Date >= monthStart && o.Date < monthEnd)
                .Select(o => o.ClientId)
                .Distinct()
                .Count();
            var churnedInMonth = ordersByMonth
                .Where(o => o.Date < monthStart)
                .GroupBy(o => o.ClientId)
                .Where(g => g.Max(x => x.Date) < monthStart.AddDays(-inactiveCutoff))
                .Select(g => g.Key)
                .Distinct()
                .Count();
            var hadActivityBefore = ordersByMonth
                .Where(o => o.Date < monthStart)
                .Select(o => o.ClientId)
                .Distinct()
                .Count();
            var retentionRate = hadActivityBefore > 0
                ? Math.Round((decimal)(hadActivityBefore - churnedInMonth) / hadActivityBefore * 100, 1)
                : 100;

            retentionTrend.Add(new ChurnRetentionTrendItemDto
            {
                MonthKey = $"{monthStart.Year}-{monthStart.Month:D2}",
                Month = monthStart.ToString("MMM yyyy"),
                ActiveClients = activeInMonth,
                ChurnedClients = churnedInMonth,
                RetentionRate = retentionRate
            });
        }

        return new ClientRetentionStatsDto
        {
            TotalClientsWithOrders = totalWithOrders,
            HealthyCount = healthy,
            WarningCount = warning,
            HighRiskCount = highRisk,
            ChurnLikelyCount = churnLikely,
            RetentionTrend = retentionTrend
        };
    }

    private async Task<Dictionary<Guid, List<string?>>> LoadOrderCurrencyCodesByClientAsync(IEnumerable<Guid> clientIds)
    {
        var ids = clientIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, List<string?>>();
        }

        var rows = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && ids.Contains(o.ClientId))
            .Select(o => new { o.ClientId, o.CurrencyCode })
            .ToListAsync();

        return rows
            .GroupBy(r => r.ClientId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.CurrencyCode).ToList());
    }
}
