using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Billing;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Services;

public class BillingService : IBillingService
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoiceService _invoiceService;
    private readonly ProductionSafetyOptions _safetyOptions;
    private readonly ILogger<BillingService> _logger;

    public BillingService(IApplicationDbContext context, IInvoiceService invoiceService, IOptions<ProductionSafetyOptions> safetyOptions, ILogger<BillingService> logger)
    {
        _context = context;
        _invoiceService = invoiceService;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
        _logger = logger;
    }

    public async Task<List<BillingQueueOverviewDto>> GetBillingQueueOverviewAsync()
    {
        var clientsWithUninvoiced = await _context.LogoOrders
            .Where(o => !o.IsDeleted
                && o.Status == OrderStatus.Completed
                && o.BillingEligible
                && !o.IsInvoiced)
            .GroupBy(o => o.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(o => o.ClientChargePrice > 0 ? o.ClientChargePrice : (o.ClientPrice ?? o.Price))
            })
            .ToListAsync();

        if (!clientsWithUninvoiced.Any())
        {
            return new List<BillingQueueOverviewDto>();
        }

        var clientIds = clientsWithUninvoiced.Select(c => c.ClientId).Distinct().ToList();
        var clients = await _context.ClientProfiles
            .Include(c => c.User)
            .Where(c => clientIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);

        var currencyRows = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => !o.IsDeleted
                && o.Status == OrderStatus.Completed
                && o.BillingEligible
                && !o.IsInvoiced
                && clientIds.Contains(o.ClientId))
            .Select(o => new { o.ClientId, o.CurrencyCode })
            .ToListAsync();
        var currencyByClient = currencyRows
            .GroupBy(x => x.ClientId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => (x.CurrencyCode ?? "USD").Trim().ToUpperInvariant()).Distinct().ToList());

        return clientsWithUninvoiced
            .Where(c => clients.ContainsKey(c.ClientId))
            .Select(c =>
            {
                var client = clients[c.ClientId];
                var clientName = $"{client.User?.FirstName} {client.User?.LastName}".Trim();
                if (string.IsNullOrEmpty(clientName)) clientName = client.CompanyName ?? "Unknown";
                string? singleCurrency = null;
                if (currencyByClient.TryGetValue(c.ClientId, out var codes) && codes.Count == 1)
                    singleCurrency = codes[0];
                return new BillingQueueOverviewDto
                {
                    ClientId = c.ClientId,
                    ClientName = clientName,
                    CompanyName = client.CompanyName ?? string.Empty,
                    UninvoicedOrderCount = c.Count,
                    TotalPendingAmount = c.TotalAmount,
                    CurrencyCode = singleCurrency
                };
            })
            .OrderByDescending(x => x.TotalPendingAmount)
            .ToList();
    }

    public async Task<BillingQueueResultDto> GetBillingQueueAsync(BillingQueueFilterDto filter)
    {
        var query = _context.LogoOrders
            .Where(o => !o.IsDeleted
                && o.Status == OrderStatus.Completed
                && o.BillingEligible);

        if (filter.OnlyUninvoiced)
        {
            query = query.Where(o => !o.IsInvoiced);
        }

        if (filter.ClientId.HasValue)
        {
            query = query.Where(o => o.ClientId == filter.ClientId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            var from = filter.FromDate.Value.Date;
            query = query.Where(o => o.CompletedDate.HasValue && o.CompletedDate.Value.Date >= from);
        }

        if (filter.ToDate.HasValue)
        {
            var to = filter.ToDate.Value.Date;
            query = query.Where(o => o.CompletedDate.HasValue && o.CompletedDate.Value.Date <= to);
        }

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 250);
        var totalCount = await query.CountAsync();

        var rows = await query
            .OrderBy(o => o.CompletedDate ?? o.UpdatedAt ?? o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.Title,
                o.ClientChargePrice,
                o.ClientPrice,
                o.Price,
                o.CurrencyCode,
                o.CompletedDate
            })
            .ToListAsync();

        var orders = rows.Select(o => new BillingEligibleOrderDto
        {
            OrderId = o.Id,
            OrderNumber = NotificationFormatHelper.GetOrderNumber(o.Id),
            Title = o.Title ?? "Logo Design",
            Price = o.ClientChargePrice > 0 ? o.ClientChargePrice : (o.ClientPrice ?? o.Price),
            CurrencyCode = string.IsNullOrWhiteSpace(o.CurrencyCode) ? "USD" : o.CurrencyCode.Trim(),
            CompletedDate = o.CompletedDate
        }).ToList();

        var totalAmountPreview = await query.SumAsync(o =>
            o.ClientChargePrice > 0 ? o.ClientChargePrice : (o.ClientPrice ?? o.Price));

        return new BillingQueueResultDto
        {
            Orders = orders,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalAmountPreview = totalAmountPreview
        };
    }

    public async Task<List<BillingEligibleOrderDto>> GetEligibleOrdersForClientAsync(Guid clientId)
    {
        // API expects ClientProfile.Id; if User.Id is accidentally supplied, resolve to ClientProfile.Id
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.Id == clientId || c.UserId == clientId);
        if (clientProfile == null)
            return new List<BillingEligibleOrderDto>();
        var resolvedClientId = clientProfile.Id;

        var orders = await _context.LogoOrders
            .Where(o => o.ClientId == resolvedClientId
                && !o.IsDeleted
                && o.Status == OrderStatus.Completed
                && o.BillingEligible
                && !o.IsInvoiced)
            .OrderBy(o => o.CompletedDate ?? o.UpdatedAt ?? o.CreatedAt)
            .ToListAsync();

        return orders.Select(o => new BillingEligibleOrderDto
        {
            OrderId = o.Id,
            OrderNumber = NotificationFormatHelper.GetOrderNumber(o.Id),
            Title = o.Title ?? $"Logo Design",
            Price = o.ClientChargePrice > 0 ? o.ClientChargePrice : (o.ClientPrice ?? o.Price),
            CurrencyCode = string.IsNullOrWhiteSpace(o.CurrencyCode) ? "USD" : o.CurrencyCode.Trim(),
            CompletedDate = o.CompletedDate
        }).ToList();
    }

    public async Task<InvoiceResponseDto> CreateInvoiceFromOrdersAsync(Guid clientId, List<CreateInvoiceOrderItemDto>? orders, List<Guid>? orderIds, string? billingPeriod, Guid createdBy)
    {
        if (_safetyOptions.DisableBillingGeneration)
            throw new InvalidOperationException("Billing temporarily disabled by administrator.");

        // API expects ClientProfile.Id; if User.Id is accidentally supplied, resolve to ClientProfile.Id
        var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.Id == clientId || c.UserId == clientId);
        if (clientProfile == null)
            throw new InvalidOperationException("Client not found.");
        var resolvedClientId = clientProfile.Id;

        CreateInvoiceRequestDto request;

        if (orders != null && orders.Any())
        {
            var orderIdList = orders.Select(o => o.OrderId).Distinct().ToList();
            var invalidOrders = await _context.LogoOrders
                .Where(o => orderIdList.Contains(o.Id) && (o.ClientId != resolvedClientId || o.IsDeleted || o.Status != OrderStatus.Completed || !o.BillingEligible || o.IsInvoiced))
                .Select(o => o.Id)
                .ToListAsync();

            if (invalidOrders.Any())
            {
                throw new InvalidOperationException("One or more selected orders are not eligible for invoicing or do not belong to this client.");
            }

            request = new CreateInvoiceRequestDto
            {
                Orders = orders,
                BillingPeriod = billingPeriod,
                BillingType = null
            };
        }
        else if (orderIds != null && orderIds.Any())
        {
            var invalidOrders = await _context.LogoOrders
                .Where(o => orderIds.Contains(o.Id) && (o.ClientId != resolvedClientId || o.IsDeleted || o.Status != OrderStatus.Completed || !o.BillingEligible || o.IsInvoiced))
                .Select(o => o.Id)
                .ToListAsync();

            if (invalidOrders.Any())
            {
                throw new InvalidOperationException("One or more selected orders are not eligible for invoicing or do not belong to this client.");
            }

            request = new CreateInvoiceRequestDto
            {
                OrderIds = orderIds,
                BillingPeriod = billingPeriod,
                BillingType = null
            };
        }
        else
        {
            throw new InvalidOperationException("At least one order must be selected.");
        }

        return await _invoiceService.CreateInvoiceAsync(request, createdBy);
    }

    public async Task ProcessAutomaticInvoicingAsync(Guid? systemUserId = null)
    {
        if (_safetyOptions.DisableBillingGeneration)
        {
            _logger.LogInformation("Billing auto-invoice skipped: DisableBillingGeneration is enabled.");
            return;
        }

        var now = DateTime.UtcNow;
        var createdBy = systemUserId ?? Guid.Empty;

        // Weekly: run on Mondays, group orders by week (Sunday-Saturday)
        if (now.DayOfWeek == DayOfWeek.Monday)
        {
            var weekStart = now.AddDays(-7).Date;
            var weekEnd = weekStart.AddDays(7);
            var billingPeriod = $"Week of {weekStart:yyyy-MM-dd}";

            var weeklyClients = await _context.ClientProfiles
                .Where(c => c.BillingType == BillingType.Weekly && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var clientId in weeklyClients)
            {
                var orderIds = await _context.LogoOrders
                    .Where(o => o.ClientId == clientId
                        && !o.IsDeleted
                        && o.Status == OrderStatus.Completed
                        && o.BillingEligible
                        && !o.IsInvoiced
                        && o.CompletedDate >= weekStart
                        && o.CompletedDate < weekEnd)
                    .Select(o => o.Id)
                    .ToListAsync();

                if (orderIds.Any())
                {
                    try
                    {
                        // Billing queue safety: validate each order before invoicing
                        var validOrderIds = await ValidateBillingEligibleOrdersAsync(orderIds);
                        if (validOrderIds.Any())
                            await CreateInvoiceFromOrdersAsync(clientId, null, validOrderIds, billingPeriod, createdBy);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Billing auto-invoice failed for client {ClientId}. Continuing with other clients.", clientId);
                    }
                }
            }
        }

        // Monthly: run on 1st of month, group previous month's orders
        if (now.Day == 1)
        {
            var lastMonth = now.AddMonths(-1);
            var monthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var billingPeriod = lastMonth.ToString("MMMM yyyy");

            var monthlyClients = await _context.ClientProfiles
                .Where(c => c.BillingType == BillingType.Monthly && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var clientId in monthlyClients)
            {
                var orderIds = await _context.LogoOrders
                    .Where(o => o.ClientId == clientId
                        && !o.IsDeleted
                        && o.Status == OrderStatus.Completed
                        && o.BillingEligible
                        && !o.IsInvoiced
                        && o.CompletedDate >= monthStart
                        && o.CompletedDate < monthEnd)
                    .Select(o => o.Id)
                    .ToListAsync();

                if (orderIds.Any())
                {
                    try
                    {
                        var validOrderIds = await ValidateBillingEligibleOrdersAsync(orderIds);
                        if (validOrderIds.Any())
                            await CreateInvoiceFromOrdersAsync(clientId, null, validOrderIds, billingPeriod, createdBy);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Billing auto-invoice failed for client {ClientId}. Continuing with other clients.", clientId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Validates BillingEligible and ClientChargePrice. Skips problematic orders and logs.
    /// </summary>
    private async Task<List<Guid>> ValidateBillingEligibleOrdersAsync(List<Guid> orderIds)
    {
        var orders = await _context.LogoOrders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id) && !o.IsDeleted)
            .ToListAsync();

        var valid = new List<Guid>();
        foreach (var order in orders)
        {
            if (!order.BillingEligible)
            {
                _logger.LogWarning("BillingQueueSafety: Skipping order {OrderId} - not BillingEligible", order.Id);
                continue;
            }
            var chargePrice = order.ClientChargePrice > 0 ? order.ClientChargePrice : (order.ClientPrice ?? order.Price);
            if (chargePrice < 0)
            {
                _logger.LogWarning("BillingQueueSafety: Skipping order {OrderId} - ClientChargePrice/ClientPrice invalid", order.Id);
                continue;
            }
            if (order.Status != OrderStatus.Completed || order.IsInvoiced)
                continue;
            valid.Add(order.Id);
        }
        return valid;
    }
}
