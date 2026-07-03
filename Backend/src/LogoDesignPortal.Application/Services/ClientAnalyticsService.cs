using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class ClientAnalyticsService : IClientAnalyticsService
{
    private readonly IApplicationDbContext _context;

    private const int ActiveDaysThreshold = 15;
    private const int LowActivityDaysThreshold = 30;
    private const int InactiveDaysThreshold = 30;
    private const decimal RevenueIncreaseThreshold = 0.25m; // 25% increase
    private static readonly decimal[] RevenueMilestones = { 1000, 5000, 10000, 25000, 50000 };

    public ClientAnalyticsService(IApplicationDbContext context)
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
            name = client.CompanyName;
        }

        if (!string.IsNullOrWhiteSpace(client.CompanyName) && name != client.CompanyName)
        {
            return $"{name} ({client.CompanyName})";
        }

        return name;
    }

    public async Task<ClientAnalyticsOverviewDto> GetOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var activeCutoff = now.AddDays(-ActiveDaysThreshold);
        var inactiveCutoff = now.AddDays(-InactiveDaysThreshold);

        var totalClients = await _context.ClientProfiles.AsNoTracking().CountAsync(c => !c.IsDeleted);

        var lastOrderByClient = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt) })
            .ToListAsync();

        var clientIdsWithOrders = lastOrderByClient.Select(x => x.ClientId).ToHashSet();
        var activeCount = lastOrderByClient.Count(x => x.LastOrder >= activeCutoff);
        var inactiveCount = lastOrderByClient.Count(x => x.LastOrder < inactiveCutoff) +
            (totalClients - clientIdsWithOrders.Count);

        var topClient = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Revenue = g.Sum(o => o.Price) })
            .OrderByDescending(r => r.Revenue)
            .FirstOrDefaultAsync();

        var topClientCurrency = "USD";
        if (topClient != null)
        {
            var profileCode = await _context.ClientProfiles.AsNoTracking()
                .Where(c => c.Id == topClient.ClientId && !c.IsDeleted)
                .Select(c => c.CurrencyCode)
                .FirstOrDefaultAsync();
            var orderCurrencies = await _context.LogoOrders.AsNoTracking()
                .Where(o => !o.IsDeleted && o.ClientId == topClient.ClientId && o.Status == OrderStatus.Completed)
                .Select(o => o.CurrencyCode)
                .ToListAsync();
            topClientCurrency = ClientCurrencyHelper.ResolveClientRevenueCurrency(profileCode, orderCurrencies);
        }

        return new ClientAnalyticsOverviewDto
        {
            TotalClients = totalClients,
            ActiveClients = activeCount,
            InactiveClients = inactiveCount,
            TopClientRevenue = topClient?.Revenue ?? 0m,
            TopClientCurrencyCode = topClientCurrency
        };
    }

    public async Task<TopClientsDto> GetTopClientsAsync(int limit = 10)
    {
        var data = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(o => o.Price)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(limit)
            .ToListAsync();

        var clientIds = data.Select(x => x.ClientId).ToList();
        var clients = await _context.ClientProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(clientIds);

        var items = data.Select(x =>
        {
            var client = clients.GetValueOrDefault(x.ClientId);
            orderCurrenciesByClient.TryGetValue(x.ClientId, out var orderCodes);
            return new TopClientItemDto
            {
                ClientId = x.ClientId,
                ClientName = GetClientDisplayName(client),
                TotalOrders = x.TotalOrders,
                TotalRevenue = x.TotalRevenue,
                CurrencyCode = ClientCurrencyHelper.ResolveClientRevenueCurrency(client?.CurrencyCode, orderCodes)
            };
        }).ToList();

        return new TopClientsDto { Items = items };
    }

    public async Task<ClientRevenueTrendDto> GetClientRevenueTrendAsync(int months = 6)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var startOfPeriod = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completedOrders = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.CurrencyCode, Date = (o.UpdatedAt ?? o.CreatedAt) })
            .Where(o => o.Date >= startOfPeriod)
            .ToListAsync();

        var items = completedOrders
            .GroupBy(o => new { Year = o.Date.Year, Month = o.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ClientRevenueTrendItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Revenue = g.Sum(x => x.Price),
                OrderCount = g.Count()
            })
            .ToList();

        var currencies = completedOrders
            .Select(o => ClientCurrencyHelper.NormalizeOrDefault(o.CurrencyCode))
            .Distinct()
            .ToList();

        return new ClientRevenueTrendDto
        {
            Items = items,
            CurrencyCode = currencies.Count == 1 ? currencies[0] : "USD",
            CurrencyMixed = currencies.Count > 1
        };
    }

    public async Task<ClientMonthlyRevenueDto> GetClientMonthlyRevenueAsync(Guid clientId, int months = 12)
    {
        var client = await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == clientId && !c.IsDeleted);

        var startDate = DateTime.UtcNow.AddMonths(-months);
        var startOfPeriod = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var orders = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.ClientId == clientId && o.Status == OrderStatus.Completed)
            .Select(o => new { o.Price, o.CurrencyCode, Date = (o.UpdatedAt ?? o.CreatedAt) })
            .Where(o => o.Date >= startOfPeriod)
            .ToListAsync();

        var items = orders
            .GroupBy(o => new { Year = o.Date.Year, Month = o.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ClientMonthlyRevenueItemDto
            {
                MonthKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Revenue = g.Sum(x => x.Price)
            })
            .ToList();

        return new ClientMonthlyRevenueDto
        {
            ClientId = clientId,
            ClientName = GetClientDisplayName(client),
            Items = items,
            CurrencyCode = ClientCurrencyHelper.ResolveClientRevenueCurrency(
                client?.CurrencyCode,
                orders.Select(o => o.CurrencyCode))
        };
    }

    public async Task<InactiveClientsDto> GetInactiveClientsAsync(int inactiveDaysThreshold = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-inactiveDaysThreshold);

        var lastOrderByClient = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt) })
            .Where(x => x.LastOrder < cutoff)
            .ToListAsync();

        var clientIds = lastOrderByClient.Select(x => x.ClientId).ToList();
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);

        var now = DateTime.UtcNow;
        var items = lastOrderByClient
            .OrderBy(x => x.LastOrder)
            .Select(x =>
            {
                var lastOrder = x.LastOrder;
                var daysSince = (int)(now - lastOrder).TotalDays;
                return new InactiveClientItemDto
                {
                    ClientId = x.ClientId,
                    ClientName = GetClientDisplayName(clients.GetValueOrDefault(x.ClientId)),
                    LastOrderDate = lastOrder,
                    DaysSinceLastOrder = daysSince
                };
            })
            .ToList();

        return new InactiveClientsDto { Items = items };
    }

    public async Task<ClientLifetimeValueDto> GetClientLifetimeValueAsync()
    {
        var data = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                LifetimeRevenue = g.Sum(o => o.Price),
                TotalOrders = g.Count()
            })
            .OrderByDescending(x => x.LifetimeRevenue)
            .ToListAsync();

        var clientIds = data.Select(x => x.ClientId).ToList();
        var clients = await _context.ClientProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(clientIds);

        var items = data.Select(x =>
        {
            var client = clients.GetValueOrDefault(x.ClientId);
            orderCurrenciesByClient.TryGetValue(x.ClientId, out var orderCodes);
            return new ClientAnalyticsLifetimeValueItemDto
            {
                ClientId = x.ClientId,
                ClientName = GetClientDisplayName(client),
                LifetimeRevenue = x.LifetimeRevenue,
                TotalOrders = x.TotalOrders,
                CurrencyCode = ClientCurrencyHelper.ResolveClientRevenueCurrency(client?.CurrencyCode, orderCodes)
            };
        }).ToList();

        return new ClientLifetimeValueDto { Items = items };
    }

    public async Task<ClientGrowthDto> GetClientGrowthMetricsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var startOfTwoMonthsAgo = startOfThisMonth.AddMonths(-2);

        var completedOrders = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Select(o => new { o.ClientId, o.Price, o.CurrencyCode, Date = (o.UpdatedAt ?? o.CreatedAt) })
            .Where(o => o.Date >= startOfTwoMonthsAgo)
            .ToListAsync();

        // Last month = most recent completed month (e.g. February if we're in March)
        var lastMonth = completedOrders
            .Where(o => o.Date >= startOfLastMonth && o.Date < startOfThisMonth)
            .GroupBy(o => o.ClientId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

        // Previous month = month before last (e.g. January if we're in March)
        var previousMonth = completedOrders
            .Where(o => o.Date >= startOfTwoMonthsAgo && o.Date < startOfLastMonth)
            .GroupBy(o => o.ClientId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

        var allClientIds = lastMonth.Keys.Union(previousMonth.Keys).Distinct().ToList();
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => allClientIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(allClientIds);

        var items = new List<ClientGrowthItemDto>();
        foreach (var clientId in allClientIds)
        {
            lastMonth.TryGetValue(clientId, out var lastRev);
            previousMonth.TryGetValue(clientId, out var prevRev);

            decimal changePercent = 0;
            string status = "Stable";
            if (prevRev > 0)
            {
                changePercent = (lastRev - prevRev) / prevRev * 100;
                status = changePercent > 0 ? "Increase" : changePercent < 0 ? "Decrease" : "Stable";
            }
            else if (lastRev > 0)
            {
                changePercent = 100;
                status = "Increase";
            }

            var client = clients.GetValueOrDefault(clientId);
            orderCurrenciesByClient.TryGetValue(clientId, out var orderCodes);
            items.Add(new ClientGrowthItemDto
            {
                ClientId = clientId,
                ClientName = GetClientDisplayName(client),
                LastMonthRevenue = lastRev,
                PreviousMonthRevenue = prevRev,
                RevenueChangePercent = Math.Round(changePercent, 1),
                GrowthStatus = status,
                CurrencyCode = ClientCurrencyHelper.ResolveClientRevenueCurrency(client?.CurrencyCode, orderCodes)
            });
        }

        items = items.OrderByDescending(x => x.RevenueChangePercent).ToList();
        return new ClientGrowthDto { Items = items };
    }

    public async Task<ClientActivityDto> GetClientActivityStatusAsync()
    {
        var now = DateTime.UtcNow;
        var activeCutoff = now.AddDays(-ActiveDaysThreshold);
        var lowActivityCutoff = now.AddDays(-LowActivityDaysThreshold);

        var lastOrderByClient = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt) })
            .ToListAsync();

        var clientIds = lastOrderByClient.Select(x => x.ClientId).Distinct().ToList();
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);

        var items = lastOrderByClient.Select(x =>
        {
            var daysSince = (int)(now - x.LastOrder).TotalDays;
            string status = x.LastOrder >= activeCutoff ? "Active"
                : x.LastOrder >= lowActivityCutoff ? "LowActivity"
                : "Inactive";
            return new ClientActivityItemDto
            {
                ClientId = x.ClientId,
                ClientName = GetClientDisplayName(clients.GetValueOrDefault(x.ClientId)),
                ActivityStatus = status,
                LastOrderDate = x.LastOrder,
                DaysSinceLastOrder = daysSince
            };
        }).OrderBy(x => x.DaysSinceLastOrder).ToList();

        return new ClientActivityDto { Items = items };
    }

    public async Task<ClientAlertsDto> GetClientAlertsAsync()
    {
        var alerts = new List<ClientAlertItemDto>();
        var now = DateTime.UtcNow;
        var inactiveCutoff = now.AddDays(-InactiveDaysThreshold);

        var clientRevenueRaw = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalRevenue = g.Sum(o => o.Price),
                LastOrder = g.Max(o => o.UpdatedAt ?? o.CreatedAt)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(20)
            .ToListAsync();

        var alertClientIds = clientRevenueRaw.Select(x => x.ClientId).ToList();
        var alertClients = await _context.ClientProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => alertClientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);
        var orderCurrenciesByClient = await LoadOrderCurrencyCodesByClientAsync(alertClientIds);

        var clientRevenue = clientRevenueRaw
            .Select(x => new
            {
                x.ClientId,
                Client = alertClients.GetValueOrDefault(x.ClientId),
                x.TotalRevenue,
                x.LastOrder
            })
            .ToList();

        var lastMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var prevMonthStart = lastMonthStart.AddMonths(-1);

        var lastMonthRev = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= lastMonthStart && (o.UpdatedAt ?? o.CreatedAt) < lastMonthStart.AddMonths(1))
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Revenue = g.Sum(o => o.Price) })
            .ToDictionaryAsync(x => x.ClientId, x => x.Revenue);

        var prevMonthRev = await _context.LogoOrders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .Where(o => (o.UpdatedAt ?? o.CreatedAt) >= prevMonthStart && (o.UpdatedAt ?? o.CreatedAt) < lastMonthStart)
            .GroupBy(o => o.ClientId)
            .Select(g => new { ClientId = g.Key, Revenue = g.Sum(o => o.Price) })
            .ToDictionaryAsync(x => x.ClientId, x => x.Revenue);

        foreach (var c in clientRevenue)
        {
            var clientName = GetClientDisplayName(c.Client);
            orderCurrenciesByClient.TryGetValue(c.ClientId, out var orderCodes);
            var clientCurrency = ClientCurrencyHelper.ResolveClientRevenueCurrency(c.Client?.CurrencyCode, orderCodes);
            var daysSince = (int)(now - c.LastOrder).TotalDays;

            if (daysSince >= InactiveDaysThreshold)
            {
                alerts.Add(new ClientAlertItemDto
                {
                    AlertType = "Inactive",
                    Message = $"Client {clientName} has not ordered for {daysSince} days.",
                    ClientId = c.ClientId,
                    ClientName = clientName,
                    OccurredAt = c.LastOrder,
                    CurrencyCode = clientCurrency
                });
            }

            prevMonthRev.TryGetValue(c.ClientId, out var prevRev);
            lastMonthRev.TryGetValue(c.ClientId, out var lastRev);
            if (prevRev > 0 && lastRev > prevRev)
            {
                var pct = (lastRev - prevRev) / prevRev;
                if (pct >= RevenueIncreaseThreshold)
                {
                    alerts.Add(new ClientAlertItemDto
                    {
                        AlertType = "RevenueIncrease",
                        Message = $"Client {clientName} revenue increased by {pct * 100:F0}% compared to previous month.",
                        ClientId = c.ClientId,
                        ClientName = clientName,
                        OccurredAt = now,
                        CurrencyCode = clientCurrency
                    });
                }
            }

            var highestMilestone = RevenueMilestones
                .Where(m => c.TotalRevenue >= m)
                .OrderByDescending(m => m)
                .FirstOrDefault();
            if (highestMilestone > 0 && c.TotalRevenue < highestMilestone * 1.2m)
            {
                alerts.Add(new ClientAlertItemDto
                {
                    AlertType = "Milestone",
                    Message = $"Client {clientName} reached revenue milestone of {clientCurrency} {highestMilestone:N0}.",
                    ClientId = c.ClientId,
                    ClientName = clientName,
                    OccurredAt = c.LastOrder,
                    CurrencyCode = clientCurrency
                });
            }
        }

        var sorted = alerts.OrderByDescending(a => a.OccurredAt).Take(50).ToList();
        return new ClientAlertsDto { Items = sorted };
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

    public async Task<List<ClientDropdownItemDto>> GetClientsForDropdownAsync()
    {
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CompanyName)
            .Select(c => new ClientDropdownItemDto
            {
                ClientId = c.Id,
                ClientName = c.User != null
                    ? $"{c.User.FirstName} {c.User.LastName} ({c.CompanyName})".Trim()
                    : c.CompanyName
            })
            .ToListAsync();

        return clients;
    }
}
