using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Common;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly ProductionSafetyOptions _safetyOptions;
    private readonly IReadModelCacheVersions _readModelCache;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IApplicationDbContext context,
        IMapper mapper,
        INotificationService notificationService,
        IRealtimeEntityUpdateSender entityUpdateSender,
        IOptions<ProductionSafetyOptions> safetyOptions,
        IReadModelCacheVersions readModelCache,
        ICurrencyService currencyService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
        _readModelCache = readModelCache;
        _currencyService = currencyService;
        _logger = logger;
    }

    /// <summary>Invalidates cached order lists, financial aggregates, and admin analytics when invoicing state changes.</summary>
    private void InvalidateFinancialReadModels()
    {
        _readModelCache.BumpOrders();
        _readModelCache.BumpAnalytics();
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy)
    {
        if (_safetyOptions.DisableBillingGeneration)
        {
            throw new InvalidOperationException("Billing temporarily disabled by administrator.");
        }

        // Determine which orders to include
        var orderIds = new List<Guid>();
        Dictionary<Guid, decimal>? orderPrices = null; // OrderId -> editable price

        // New: support Orders with editable prices (takes precedence)
        if (request.Orders != null && request.Orders.Any())
        {
            // Handle duplicate order IDs: group by OrderId and take last price to avoid crash
            var uniqueOrders = request.Orders
                .GroupBy(o => o.OrderId)
                .Select(g => g.Last())
                .ToList();
            orderIds.AddRange(uniqueOrders.Select(o => o.OrderId));
            orderPrices = uniqueOrders.ToDictionary(o => o.OrderId, o => o.Price);
        }
        else
        {
            // Backward compatibility: support single OrderId
            if (request.OrderId.HasValue)
            {
                orderIds.Add(request.OrderId.Value);
            }

            // New: support multiple OrderIds
            if (request.OrderIds != null && request.OrderIds.Any())
            {
                orderIds.AddRange(request.OrderIds);
            }
        }

        // Validate orders exist and get client
        Guid? clientId = null;
        if (orderIds.Any())
        {
            var orders = await _context.LogoOrders
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Where(o => orderIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync();

            if (orders.Count != orderIds.Count)
            {
                throw new InvalidOperationException("One or more orders not found.");
            }

            // All orders must be Completed, BillingEligible, and not yet invoiced
            var nonCompletedOrders = orders.Where(o => o.Status != OrderStatus.Completed).ToList();
            if (nonCompletedOrders.Any())
            {
                throw new InvalidOperationException("All orders must have status Completed before an invoice can be created.");
            }

            var alreadyInvoiced = orders.Where(o => o.IsInvoiced).ToList();
            if (alreadyInvoiced.Any())
            {
                throw new InvalidOperationException("One or more of these orders have already been invoiced.");
            }

            var notEligible = orders.Where(o => !o.BillingEligible).ToList();
            if (notEligible.Any())
            {
                throw new InvalidOperationException("One or more orders are not billing eligible.");
            }

            // Ensure all orders belong to the same client
            var firstClientId = orders.First().ClientId;
            if (orders.Any(o => o.ClientId != firstClientId))
            {
                throw new InvalidOperationException("All orders must belong to the same client.");
            }

            clientId = firstClientId;
        }
        else if (request.ManualItems == null || !request.ManualItems.Any())
        {
            throw new InvalidOperationException("At least one order or manual item must be provided.");
        }

        // For manual items, get client from first item's order if available
        if (!clientId.HasValue && request.ManualItems != null && request.ManualItems.Any(mi => mi.OrderId.HasValue))
        {
            var firstOrderWithId = request.ManualItems.First(mi => mi.OrderId.HasValue);
            var order = await _context.LogoOrders
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.Id == firstOrderWithId.OrderId && !o.IsDeleted);

            if (order != null)
            {
                clientId = order.ClientId;
            }
        }

        if (!clientId.HasValue)
        {
            throw new InvalidOperationException("Unable to determine client. Please provide at least one order.");
        }

        var client = await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == clientId.Value && !c.IsDeleted);

        if (client == null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        // Generate invoice number
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        // Create invoice items
        var invoiceItems = new List<InvoiceOrder>();
        decimal totalAmount = 0;

        // Add order-based items
        if (orderIds.Any())
        {
            var orders = await _context.LogoOrders
                .Where(o => orderIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync();

            foreach (var order in orders)
            {
                // Financial safety guard: validate order status, price fields, amounts
                if (order.Status != OrderStatus.Completed)
                {
                    _logger.LogWarning("FinancialSafety: Skipping order {OrderId} - status is {Status}, expected Completed", order.Id, order.Status);
                    throw new InvalidOperationException($"Order {order.Id} must have status Completed for invoicing.");
                }
                var chargePrice = order.ClientChargePrice > 0 ? order.ClientChargePrice : (order.ClientPrice ?? order.Price);
                if (chargePrice < 0)
                {
                    _logger.LogWarning("FinancialSafety: Order {OrderId} has negative price. Skipping invoice creation.", order.Id);
                    throw new InvalidOperationException($"Order {order.Id} has invalid (negative) price. Cannot create invoice.");
                }

                // Use editable price from request if provided, else ClientChargePrice, else ClientPrice, else Price
                decimal orderAmount;
                if (orderPrices != null && orderPrices.TryGetValue(order.Id, out var editablePrice))
                {
                    if (editablePrice < 0)
                    {
                        throw new InvalidOperationException($"Invoice item price for order {order.Id} cannot be negative.");
                    }

                    if (editablePrice > 10_000_000)
                    {
                        throw new InvalidOperationException($"Invoice item price for order {order.Id} exceeds maximum allowed (10,000,000).");
                    }

                    orderAmount = editablePrice;
                }
                else
                {
                    orderAmount = order.ClientChargePrice > 0 ? order.ClientChargePrice : (order.ClientPrice ?? order.Price);
                }

                var item = new InvoiceOrder
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = Guid.Empty, // Will be set after invoice creation
                    OrderId = order.Id,
                    Description = order.Title ?? $"Logo Design - Order #{order.Id.ToString().Substring(0, 8)}",
                    Amount = orderAmount,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                invoiceItems.Add(item);
                totalAmount += orderAmount;
            }
        }

        // Manual lines that reference an order must not duplicate an order already on this invoice request
        if (request.ManualItems != null)
        {
            var manualOrderIds = request.ManualItems.Where(m => m.OrderId.HasValue).Select(m => m.OrderId!.Value).ToList();
            if (manualOrderIds.Any(id => orderIds.Contains(id)))
            {
                throw new InvalidOperationException("An order cannot be listed both as a selected order and as a manual line with the same order.");
            }

            if (manualOrderIds.Any())
            {
                var manualOrders = await _context.LogoOrders
                    .Where(o => manualOrderIds.Contains(o.Id) && !o.IsDeleted)
                    .ToListAsync();
                if (manualOrders.Count != manualOrderIds.Distinct().Count())
                {
                    throw new InvalidOperationException("One or more manual line order references were not found.");
                }

                await ValidateOrdersEligibleForInvoicingAsync(manualOrders, client.Id);
            }
        }

        // Add manual items
        if (request.ManualItems != null && request.ManualItems.Any())
        {
            foreach (var manualItem in request.ManualItems)
            {
                if (manualItem.Amount < 0)
                {
                    throw new InvalidOperationException("Manual invoice item amount cannot be negative.");
                }

                if (manualItem.Amount > 10_000_000)
                {
                    throw new InvalidOperationException("Manual invoice item amount exceeds maximum allowed (10,000,000).");
                }

                var item = new InvoiceOrder
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = Guid.Empty, // Will be set after invoice creation
                    OrderId = manualItem.OrderId, // Can be null for non-order items
                    Description = manualItem.Description,
                    Amount = manualItem.Amount,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                invoiceItems.Add(item);
                totalAmount += manualItem.Amount;
            }
        }

        List<LogoOrder> ordersForRate = orderIds.Count > 0
            ? await _context.LogoOrders
                .Where(o => orderIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync()
            : new List<LogoOrder>();

        // Create invoice
        var billingType = request.BillingType ?? client.BillingType;
        var taxAmount = request.TaxAmount ?? 0;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            InvoiceNumber = invoiceNumber,
            Amount = totalAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount + taxAmount,
            Status = InvoiceStatus.Pending,
            BillingType = billingType,
            BillingPeriod = request.BillingPeriod,
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await ApplyExchangeRateSnapshotAsync(invoice, client, ordersForRate);

        // Atomic transaction: invoice + items + order updates - all or nothing
        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            _context.Invoices.Add(invoice);

            foreach (var item in invoiceItems)
            {
                item.InvoiceId = invoice.Id;
                _context.InvoiceOrders.Add(item);
            }

            var orderIdsToMarkInvoiced = CollectOrderIdsToMarkInvoiced(orderIds, request.ManualItems);
            var ordersToUpdate = await _context.LogoOrders.Where(o => orderIdsToMarkInvoiced.Contains(o.Id)).ToListAsync(ct);
            foreach (var order in ordersToUpdate)
            {
                order.IsInvoiced = true;
                order.InvoiceId = invoice.Id;
                order.InvoicedDate = DateTime.UtcNow;
            }

            await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Created, createdBy, "Invoice created");
            await _context.SaveChangesAsync(ct);
        });

        InvalidateFinancialReadModels();

        // Structured logging for production monitoring
        _logger.LogInformation("InvoiceGenerated. InvoiceId={InvoiceId}, OrderIds={OrderIds}, UserId={UserId}, Timestamp={Timestamp}",
            invoice.Id, string.Join(",", orderIds), createdBy, DateTime.UtcNow);

        await NotifyClientInvoiceReceivedAsync(client.UserId, invoice, invoiceItems, createdBy);

        return await GetInvoiceByIdAsync(invoice.Id) ?? throw new InvalidOperationException("Failed to create invoice.");
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
                .ThenInclude(io => io.Order)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return null;
        }

        return MapToInvoiceResponseDto(invoice);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdWithAccessAsync(Guid invoiceId, Guid userId, string? userRole)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
                .ThenInclude(io => io.Order)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return null;
        }

        if (!PaymentInvoiceAccessHelper.CanAccessInvoice(invoice, userId, userRole))
        {
            return null;
        }

        return MapToInvoiceResponseDto(invoice);
    }

    public async Task<PagedResultDto<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole, InvoiceQueryFilterDto? filters = null, int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = BuildFilteredInvoicesQuery(userId, userRole, filters);
        var total = await query.CountAsync();

        var pageIds = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => i.Id)
            .ToListAsync();

        if (pageIds.Count == 0)
        {
            return new PagedResultDto<InvoiceResponseDto>
            {
                Items = new List<InvoiceResponseDto>(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // IgnoreQueryFilters: ClientProfile soft-delete filter turns Include(Client) into an inner join,
        // hiding invoices whose client was deleted. pageIds already excludes deleted invoices.
        var invoices = await _context.Invoices
            .IgnoreQueryFilters()
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
            .Where(i => pageIds.Contains(i.Id))
            .ToListAsync();

        var orderLineInfo = await LoadInvoiceOrderLineInfoAsync(invoices);

        await BatchUpdateOverdueInvoicesAsync(invoices);

        var idOrder = pageIds.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        var sorted = invoices.OrderBy(i => idOrder[i.Id]).ToList();
        var items = sorted.Select(i => MapToInvoiceResponseDto(i, orderLineInfo)).ToList();

        return new PagedResultDto<InvoiceResponseDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesForExportAsync(Guid? userId, string? userRole, InvoiceQueryFilterDto? filters = null, int maxRows = 500)
    {
        maxRows = Math.Clamp(maxRows, 1, 500);
        var query = BuildFilteredInvoicesQuery(userId, userRole, filters);

        var ids = await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(maxRows)
            .Select(i => i.Id)
            .ToListAsync();

        if (ids.Count == 0)
        {
            return new List<InvoiceResponseDto>();
        }

        var invoices = await _context.Invoices
            .IgnoreQueryFilters()
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
            .Where(i => ids.Contains(i.Id))
            .ToListAsync();

        var orderLineInfo = await LoadInvoiceOrderLineInfoAsync(invoices);
        await BatchUpdateOverdueInvoicesAsync(invoices);

        var idOrder = ids.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        var sorted = invoices.OrderBy(i => idOrder[i.Id]).ToList();
        return sorted.Select(i => MapToInvoiceResponseDto(i, orderLineInfo)).ToList();
    }

    private IQueryable<Invoice> BuildFilteredInvoicesQuery(Guid? userId, string? userRole, InvoiceQueryFilterDto? filters)
    {
        var query = _context.Invoices
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        if (userRole == "Client" && userId.HasValue)
        {
            query = query.Where(i => i.Client.UserId == userId.Value);
        }
        else if (userRole is not "Admin" and not "SuperAdmin")
        {
            query = query.Where(_ => false);
        }

        if (filters == null)
        {
            return query;
        }

        if (filters.ClientId.HasValue)
        {
            query = query.Where(i => i.ClientId == filters.ClientId.Value);
        }

        if (filters.BillingType.HasValue)
        {
            query = query.Where(i => i.BillingType == filters.BillingType.Value);
        }

        if (filters.ExcludePaid)
        {
            query = query.Where(i => i.Status != InvoiceStatus.Paid);
        }
        else if (filters.Status.HasValue)
        {
            query = query.Where(i => i.Status == filters.Status.Value);
        }

        if (filters.IssueDateFrom.HasValue)
        {
            var from = filters.IssueDateFrom.Value.Date;
            query = query.Where(i => i.IssueDate.Date >= from);
        }
        if (filters.IssueDateTo.HasValue)
        {
            var to = filters.IssueDateTo.Value.Date;
            query = query.Where(i => i.IssueDate.Date <= to);
        }

        return query;
    }

    private async Task<Dictionary<Guid, (string? Title, DateTime CreatedAt, string CurrencyCode)>> LoadInvoiceOrderLineInfoAsync(List<Invoice> invoices)
    {
        var orderIds = invoices
            .SelectMany(i => i.InvoiceOrders.Where(io => io.OrderId.HasValue).Select(io => io.OrderId!.Value))
            .Distinct()
            .ToList();
        if (orderIds.Count == 0)
        {
            return new Dictionary<Guid, (string?, DateTime, string)>();
        }

        return await _context.LogoOrders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .Select(o => new { o.Id, o.Title, o.CreatedAt, o.CurrencyCode })
            .ToDictionaryAsync(x => x.Id, x => ((string?)x.Title, x.CreatedAt, x.CurrencyCode));
    }

    public async Task<InvoiceResponseDto> GenerateFlexibleInvoiceAsync(GenerateFlexibleInvoiceRequestDto request, Guid createdBy)
    {
        if (_safetyOptions.DisableBillingGeneration)
        {
            throw new InvalidOperationException("Billing temporarily disabled by administrator.");
        }

        var hasDateRange = request.FromDate.HasValue && request.ToDate.HasValue;
        var hasSelection = request.SelectedOrderIds != null && request.SelectedOrderIds.Any();
        if (!hasDateRange && !hasSelection)
        {
            throw new InvalidOperationException("Either fromDate/toDate or selectedOrderIds must be provided.");
        }

        if (hasDateRange && request.FromDate!.Value.Date > request.ToDate!.Value.Date)
        {
            throw new InvalidOperationException("fromDate cannot be greater than toDate.");
        }

        var baseQuery = _context.LogoOrders
            .Where(o => !o.IsDeleted
                && o.ClientId == request.ClientId
                && o.Status == OrderStatus.Completed
                && o.BillingEligible
                && !o.IsInvoiced);

        if (hasSelection)
        {
            var selectedIds = request.SelectedOrderIds!.Distinct().ToList();
            baseQuery = baseQuery.Where(o => selectedIds.Contains(o.Id));
        }
        else
        {
            var from = request.FromDate!.Value.Date;
            var to = request.ToDate!.Value.Date;
            baseQuery = baseQuery.Where(o => o.CompletedDate.HasValue
                && o.CompletedDate.Value.Date >= from
                && o.CompletedDate.Value.Date <= to);
        }

        var orders = await baseQuery.OrderBy(o => o.CompletedDate ?? o.CreatedAt).ToListAsync();
        if (!orders.Any())
        {
            throw new InvalidOperationException("No billing eligible uninvoiced orders found for the provided criteria.");
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            BillingType = BillingType.Manual,
            BillingPeriod = hasDateRange
                ? $"{request.FromDate:yyyy-MM-dd} to {request.ToDate:yyyy-MM-dd}"
                : $"Custom Selection ({orders.Count} Orders)",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = InvoiceStatus.Pending,
            Notes = request.Notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        var items = orders.Select(order =>
        {
            var orderNo = NotificationFormatHelper.GetOrderNumber(order.Id);
            var title = string.IsNullOrWhiteSpace(order.Title) ? "Logo design" : order.Title.Trim();
            return new InvoiceOrder
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                OrderId = order.Id,
                Description = $"{title} — Order #{orderNo}",
                Amount = order.ClientChargePrice > 0 ? order.ClientChargePrice : (order.ClientPrice ?? order.Price),
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }).ToList();

        invoice.Amount = items.Sum(i => i.Amount);
        invoice.TaxAmount = 0;
        invoice.TotalAmount = invoice.Amount + invoice.TaxAmount;

        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.Id == request.ClientId && !c.IsDeleted)
            ?? throw new InvalidOperationException("Client not found.");

        await ApplyExchangeRateSnapshotAsync(invoice, client, orders);

        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            _context.Invoices.Add(invoice);
            _context.InvoiceOrders.AddRange(items);

            foreach (var order in orders)
            {
                order.IsInvoiced = true;
                order.InvoiceId = invoice.Id;
                order.InvoicedDate = DateTime.UtcNow;
            }

            await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Created, createdBy, "Flexible manual invoice generated");
            await _context.SaveChangesAsync(ct);
        });

        InvalidateFinancialReadModels();

        // Notify client after successful invoice generation
        var clientUserId = await _context.ClientProfiles
            .Where(c => c.Id == request.ClientId && !c.IsDeleted)
            .Select(c => c.UserId)
            .FirstOrDefaultAsync();
        if (clientUserId != Guid.Empty)
        {
            await NotifyClientInvoiceReceivedAsync(clientUserId, invoice, items, createdBy);
        }

        return await GetInvoiceByIdAsync(invoice.Id) ?? throw new InvalidOperationException("Failed to create flexible invoice.");
    }

    public async Task<InvoiceResponseDto> EditInvoiceItemsAsync(Guid invoiceId, EditInvoiceItemsRequestDto request, Guid updatedBy)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot edit items of a paid invoice.");
        }

        var previousTotalAmount = invoice.TotalAmount;
        var updateChangeKeys = new List<string>();

        var addOrderIds = request.AddOrderIds?.Distinct().ToList() ?? new List<Guid>();
        List<LogoOrder>? ordersToAdd = null;
        if (addOrderIds.Any())
        {
            var existingOnInvoice = invoice.InvoiceOrders
                .Where(io => io.OrderId.HasValue)
                .Select(io => io.OrderId!.Value)
                .ToHashSet();
            if (addOrderIds.Any(id => existingOnInvoice.Contains(id)))
            {
                throw new InvalidOperationException("One or more orders are already on this invoice.");
            }

            ordersToAdd = await _context.LogoOrders
                .Where(o => addOrderIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync();
            if (ordersToAdd.Count != addOrderIds.Count)
            {
                throw new InvalidOperationException("One or more orders not found.");
            }

            await ValidateOrdersEligibleForInvoicingAsync(ordersToAdd, invoice.ClientId);
        }

        if (request.AddManualItems != null && request.AddManualItems.Any())
        {
            var manualOrderIds = request.AddManualItems.Where(m => m.OrderId.HasValue).Select(m => m.OrderId!.Value).Distinct().ToList();
            if (manualOrderIds.Any(id => addOrderIds.Contains(id)))
            {
                throw new InvalidOperationException("An order cannot be added both as a logo line and as a manual line with the same order.");
            }

            var onInvoiceOrderIds = invoice.InvoiceOrders
                .Where(io => io.OrderId.HasValue)
                .Select(io => io.OrderId!.Value)
                .ToHashSet();
            if (manualOrderIds.Any(id => onInvoiceOrderIds.Contains(id)))
            {
                throw new InvalidOperationException("One or more orders are already on this invoice.");
            }

            if (manualOrderIds.Any())
            {
                var manualOrders = await _context.LogoOrders
                    .Where(o => manualOrderIds.Contains(o.Id) && !o.IsDeleted)
                    .ToListAsync();
                if (manualOrders.Count != manualOrderIds.Count)
                {
                    throw new InvalidOperationException("One or more manual line order references were not found.");
                }

                await ValidateOrdersEligibleForInvoicingAsync(manualOrders, invoice.ClientId);
            }
        }

        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            if (request.RemoveItemIds != null && request.RemoveItemIds.Any())
            {
                var itemIds = request.RemoveItemIds.Distinct().ToList();
                var itemsToRemove = invoice.InvoiceOrders.Where(io => itemIds.Contains(io.Id)).ToList();

                foreach (var item in itemsToRemove)
                {
                    if (item.OrderId.HasValue)
                    {
                        var order = await _context.LogoOrders.FirstOrDefaultAsync(o => o.Id == item.OrderId.Value, ct);
                        if (order != null)
                        {
                            order.IsInvoiced = false;
                            order.InvoiceId = null;
                            order.InvoicedDate = null;
                        }
                    }
                }

                _context.InvoiceOrders.RemoveRange(itemsToRemove);
            }

            if (ordersToAdd != null && ordersToAdd.Any())
            {
                foreach (var order in ordersToAdd)
                {
                    var orderNo = NotificationFormatHelper.GetOrderNumber(order.Id);
                    var title = string.IsNullOrWhiteSpace(order.Title) ? "Logo design" : order.Title.Trim();
                    var amount = order.ClientChargePrice > 0 ? order.ClientChargePrice : (order.ClientPrice ?? order.Price);
                    _context.InvoiceOrders.Add(new InvoiceOrder
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        OrderId = order.Id,
                        Description = $"{title} — Order #{orderNo}",
                        Amount = amount,
                        CreatedBy = updatedBy,
                        CreatedAt = DateTime.UtcNow
                    });
                    order.IsInvoiced = true;
                    order.InvoiceId = invoice.Id;
                    order.InvoicedDate = DateTime.UtcNow;
                }
            }

            if (request.AddManualItems != null && request.AddManualItems.Any())
            {
                foreach (var x in request.AddManualItems)
                {
                    if (x.Amount < 0)
                    {
                        throw new InvalidOperationException("Manual invoice item amount cannot be negative.");
                    }

                    if (x.Amount > 10_000_000)
                    {
                        throw new InvalidOperationException("Manual invoice item amount exceeds maximum allowed (10,000,000).");
                    }

                    _context.InvoiceOrders.Add(new InvoiceOrder
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        OrderId = x.OrderId,
                        Description = x.Description,
                        Amount = x.Amount,
                        CreatedBy = updatedBy,
                        CreatedAt = DateTime.UtcNow
                    });

                    if (x.OrderId.HasValue)
                    {
                        var order = await _context.LogoOrders.FirstOrDefaultAsync(o => o.Id == x.OrderId.Value, ct);
                        if (order != null)
                        {
                            order.IsInvoiced = true;
                            order.InvoiceId = invoice.Id;
                            order.InvoicedDate = DateTime.UtcNow;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(ct);

            var subtotal = await _context.InvoiceOrders
                .Where(io => io.InvoiceId == invoice.Id && !io.IsDeleted)
                .SumAsync(io => io.Amount, ct);

            invoice.Amount = subtotal;
            invoice.TotalAmount = subtotal + invoice.TaxAmount;
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(ct);
        });

        var logosAddedCount = addOrderIds.Count
            + (request.AddManualItems?.Count(m => m.OrderId.HasValue) ?? 0);
        if (logosAddedCount > 0)
        {
            updateChangeKeys.Add(InvoiceUpdateChangeLabels.NewLogoAdded);
        }

        var removedCount = request.RemoveItemIds?.Distinct().Count() ?? 0;
        if (removedCount > 0)
        {
            updateChangeKeys.Add(InvoiceUpdateChangeLabels.LineItemRemoved);
        }

        if (previousTotalAmount != invoice.TotalAmount)
        {
            updateChangeKeys.Add(InvoiceUpdateChangeLabels.PriceUpdated);
        }

        var logDetail = updateChangeKeys.Count > 0
            ? InvoiceUpdateChangeLabels.FormatNotificationMessage(
                NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber),
                updateChangeKeys)
            : "Invoice items edited";
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Updated, updatedBy, logDetail);
        await _context.SaveChangesAsync();

        if (updateChangeKeys.Count > 0 && invoice.Client?.UserId is Guid clientUserId)
        {
            await NotifyClientInvoiceUpdatedAsync(clientUserId, invoice, updateChangeKeys, updatedBy);
        }

        InvalidateFinancialReadModels();

        return await GetInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to update invoice items.");
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByClientAsync(Guid clientId)
    {
        const int maxInvoicesForClientDetail = 150;
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            return new List<InvoiceResponseDto>();
        }

        var invoices = await _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
            .Where(i => i.ClientId == client.Id && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Take(maxInvoicesForClientDetail)
            .ToListAsync();

        var orderLineInfo = await LoadInvoiceOrderLineInfoAsync(invoices);
        await BatchUpdateOverdueInvoicesAsync(invoices);

        return invoices.Select(i => MapToInvoiceResponseDto(i, orderLineInfo)).ToList();
    }

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequestDto request, Guid updatedBy)
    {
        if (_safetyOptions.DisableInvoiceEditing)
        {
            throw new InvalidOperationException("Invoice editing temporarily disabled by administrator.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot edit a paid invoice.");
        }

        var previousDueDate = invoice.DueDate;
        var previousTotalAmount = invoice.TotalAmount;
        var previousTaxAmount = invoice.TaxAmount;
        var updateChangeKeys = new List<string>();

        // Update invoice fields
        if (request.BillingType.HasValue)
        {
            invoice.BillingType = request.BillingType.Value;
        }

        if (request.TaxAmount.HasValue && request.TaxAmount.Value != previousTaxAmount)
        {
            invoice.TaxAmount = request.TaxAmount.Value;
            invoice.TotalAmount = invoice.Amount + invoice.TaxAmount;
            updateChangeKeys.Add(InvoiceUpdateChangeLabels.PriceUpdated);
        }

        if (request.DueDate.HasValue)
        {
            var newDueDate = request.DueDate.Value;
            if (previousDueDate != newDueDate)
            {
                invoice.DueDate = newDueDate;
                updateChangeKeys.Add(
                    newDueDate > (previousDueDate ?? DateTime.MinValue)
                        ? InvoiceUpdateChangeLabels.DueDateExtended
                        : InvoiceUpdateChangeLabels.DueDateUpdated);
            }

            // If due date is in the future and invoice was overdue, reset to pending
            if (invoice.DueDate > DateTime.UtcNow && invoice.Status == InvoiceStatus.Overdue)
            {
                invoice.Status = InvoiceStatus.Pending;
            }
        }

        if (request.PaymentMethod != null)
        {
            invoice.PaymentMethod = request.PaymentMethod;
        }

        if (request.Notes != null)
        {
            invoice.Notes = request.Notes;
        }

        // Update individual items if provided
        if (request.Items != null && request.Items.Any())
        {
            foreach (var itemUpdate in request.Items)
            {
                var existingItem = invoice.InvoiceOrders.FirstOrDefault(io => io.Id == itemUpdate.Id);
                if (existingItem != null)
                {
                    if (itemUpdate.Description != null)
                    {
                        existingItem.Description = itemUpdate.Description;
                    }
                    if (itemUpdate.Amount.HasValue)
                    {
                        if (itemUpdate.Amount.Value < 0)
                        {
                            throw new InvalidOperationException("Invoice item amount cannot be negative.");
                        }

                        if (existingItem.Amount != itemUpdate.Amount.Value)
                        {
                            updateChangeKeys.Add(InvoiceUpdateChangeLabels.PriceUpdated);
                        }

                        existingItem.Amount = itemUpdate.Amount.Value;
                    }
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.UpdatedBy = updatedBy;
                }
            }

            // Recalculate totals from items
            invoice.Amount = invoice.InvoiceOrders.Sum(io => io.Amount);
            invoice.TotalAmount = invoice.Amount + invoice.TaxAmount;
        }

        if (previousTotalAmount != invoice.TotalAmount
            && !updateChangeKeys.Contains(InvoiceUpdateChangeLabels.PriceUpdated))
        {
            updateChangeKeys.Add(InvoiceUpdateChangeLabels.PriceUpdated);
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = updatedBy;

        var logDetail = updateChangeKeys.Count > 0
            ? InvoiceUpdateChangeLabels.FormatNotificationMessage(
                NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber),
                updateChangeKeys)
            : "Invoice updated";
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Updated, updatedBy, logDetail);

        await _context.SaveChangesAsync();

        if (updateChangeKeys.Count > 0 && invoice.Client?.UserId is Guid clientUserId)
        {
            await NotifyClientInvoiceUpdatedAsync(clientUserId, invoice, updateChangeKeys, updatedBy);
        }

        InvalidateFinancialReadModels();

        return await GetInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to update invoice.");
    }

    public async Task<InvoiceResponseDto> MarkInvoiceAsPaidAsync(Guid invoiceId, string? paymentMethod, Guid? performedBy)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Invoice is already marked as paid.");
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.PaymentMethod = paymentMethod ?? invoice.PaymentMethod;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = performedBy ?? invoice.CreatedBy;

        // Create audit log
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Paid, performedBy, $"Invoice marked as paid. Payment method: {paymentMethod ?? "Not specified"}");

        await _context.SaveChangesAsync();

        InvalidateFinancialReadModels();

        _logger.LogInformation("PaymentRecorded. InvoiceId={InvoiceId}, UserId={UserId}, Timestamp={Timestamp}",
            invoiceId, performedBy ?? invoice.CreatedBy, DateTime.UtcNow);

        // Notify Admin and SuperAdmin: Payment received
        try
        {
            var invoiceWithOrders = await _context.Invoices
                .Include(i => i.InvoiceOrders)
                .ThenInclude(io => io.Order)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);
            var firstOrder = invoiceWithOrders?.InvoiceOrders?.FirstOrDefault(io => io.OrderId.HasValue)?.Order;
            var title = "Payment Received";
            var message = firstOrder != null
                ? $"Payment received for order (#{NotificationFormatHelper.GetOrderNumber(firstOrder.Id)})"
                : $"Payment received for invoice (#{NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber)})";
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, invoice.Id, performedBy);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Success, NotificationReferenceType.Invoice, invoice.Id, performedBy);
        }
        catch
        {
            // Must not fail invoice update
        }

        return await GetInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to update invoice.");
    }

    public async Task<bool> SendInvoiceAsync(Guid invoiceId, Guid? performedBy)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return false;
        }

        // Create audit log
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Sent, performedBy, "Invoice sent to client");

        await _context.SaveChangesAsync();

        InvalidateFinancialReadModels();

        // TODO: Implement email sending logic
        // For now, just return true
        return true;
    }

    public async Task<List<InvoiceLogDto>> GetInvoiceLogsAsync(Guid invoiceId)
    {
        var logs = await _context.InvoiceLogs
            .Include(l => l.Invoice)
            .Where(l => l.InvoiceId == invoiceId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var performerIds = logs.Where(l => l.PerformedBy.HasValue).Select(l => l.PerformedBy!.Value).Distinct().ToList();
        var users = performerIds.Count > 0
            ? await _context.Users.Where(u => performerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim())
            : new Dictionary<Guid, string>();

        return logs.Select(log =>
        {
            var dto = new InvoiceLogDto
            {
                Id = log.Id,
                Action = log.Action.ToString(),
                PerformedBy = log.PerformedBy,
                Notes = log.Notes,
                CreatedAt = log.CreatedAt
            };
            if (log.PerformedBy.HasValue && users.TryGetValue(log.PerformedBy.Value, out var name))
            {
                dto.PerformedByName = name;
            }

            return dto;
        }).ToList();
    }

    public async Task<List<InvoiceLogDto>?> GetInvoiceLogsWithAccessAsync(Guid invoiceId, Guid userId, string? userRole)
    {
        var invoice = await GetInvoiceByIdWithAccessAsync(invoiceId, userId, userRole);
        if (invoice == null)
        {
            return null;
        }
        return await GetInvoiceLogsAsync(invoiceId);
    }

    public async Task UpdateOverdueInvoicesAsync()
    {
        var overdueInvoices = await _context.Invoices
            .Where(i => !i.IsDeleted
                && i.Status != InvoiceStatus.Paid
                && i.DueDate.HasValue
                && i.DueDate.Value < DateTime.UtcNow)
            .ToListAsync();

        var anyStatusChanged = false;
        foreach (var invoice in overdueInvoices)
        {
            if (invoice.Status != InvoiceStatus.Overdue)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
                anyStatusChanged = true;

                // Create audit log (system action)
                await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Overdue, null, "Invoice automatically marked as overdue");
            }
        }

        await _context.SaveChangesAsync();

        if (anyStatusChanged)
        {
            InvalidateFinancialReadModels();
        }
    }

    // Private helper methods

    private async Task BatchUpdateOverdueInvoicesAsync(List<Invoice> invoices)
    {
        var toUpdate = invoices
            .Where(i => i.Status != InvoiceStatus.Paid
                && i.DueDate.HasValue
                && i.DueDate.Value < DateTime.UtcNow
                && i.Status != InvoiceStatus.Overdue)
            .ToList();
        if (toUpdate.Count == 0)
        {
            return;
        }

        foreach (var invoice in toUpdate)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;
            await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Overdue, null, "Invoice automatically marked as overdue");
        }
        await _context.SaveChangesAsync();
    }

    private async Task UpdateInvoiceStatusIfNeededAsync(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Paid)
        {
            return;
        }

        if (invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow && invoice.Status != InvoiceStatus.Overdue)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;
            await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Overdue, null, "Invoice automatically marked as overdue");
            await _context.SaveChangesAsync();
        }
    }

    private async Task CreateInvoiceLogAsync(Guid invoiceId, InvoiceAction action, Guid? performedBy, string? notes = null)
    {
        var log = new InvoiceLog
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Action = action,
            PerformedBy = performedBy,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = performedBy
        };

        _context.InvoiceLogs.Add(log);
    }

    private async Task NotifyClientInvoiceUpdatedAsync(
        Guid clientUserId,
        Invoice invoice,
        IReadOnlyList<string> changeKeys,
        Guid? updatedBy)
    {
        if (changeKeys.Count == 0)
        {
            return;
        }

        try
        {
            var firstOrderId = invoice.InvoiceOrders.FirstOrDefault(io => io.OrderId.HasValue)?.OrderId;
            var invoiceDisplayNumber = NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber);
            var title = "Invoice Updated";
            var message = InvoiceUpdateChangeLabels.FormatNotificationMessage(invoiceDisplayNumber, changeKeys);

            await _notificationService.CreateNotificationAsync(
                clientUserId,
                title,
                message,
                NotificationType.Info,
                firstOrderId,
                NotificationReferenceType.Invoice,
                invoice.Id,
                updatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Notification failed for InvoiceUpdated. InvoiceId={InvoiceId}. Operation succeeded.",
                invoice.Id);
        }
    }

    private async Task NotifyClientInvoiceReceivedAsync(Guid clientUserId, Invoice invoice, List<InvoiceOrder> invoiceItems, Guid? createdBy)
    {
        // SignalR/notification failures must not block invoice creation.
        try
        {
            var firstOrderId = invoiceItems.FirstOrDefault(io => io.OrderId.HasValue)?.OrderId;
            var invoiceDisplayNumber = NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber);
            var title = "Invoice Received";
            var message = $"You have received invoice #{invoiceDisplayNumber}.";

            await _notificationService.CreateNotificationAsync(
                clientUserId,
                title,
                message,
                NotificationType.Info,
                firstOrderId,
                NotificationReferenceType.Invoice,
                invoice.Id,
                createdBy
            );

            // Real-time entity update: InvoiceGenerated - client order grid HasInvoice flag updates
            foreach (var item in invoiceItems.Where(io => io.OrderId.HasValue))
            {
                await _entityUpdateSender.SendInvoiceGeneratedAsync(item.OrderId!.Value, invoice.Id, clientUserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR/Notification failed for InvoiceGenerated. InvoiceId={InvoiceId}. Operation succeeded.", invoice.Id);
        }
    }

    private InvoiceResponseDto MapToInvoiceResponseDto(Invoice invoice)
        => MapToInvoiceResponseDto(invoice, null);

    private InvoiceResponseDto MapToInvoiceResponseDto(Invoice invoice,
        IReadOnlyDictionary<Guid, (string? Title, DateTime CreatedAt, string CurrencyCode)>? orderLineInfo)
    {
        var orderIds = invoice.InvoiceOrders
            .Where(io => io.OrderId.HasValue)
            .Select(io => io.OrderId!.Value)
            .ToList();

        var firstOrderId = invoice.InvoiceOrders.FirstOrDefault(io => io.OrderId.HasValue)?.OrderId;
        Guid orderId;
        if (firstOrderId.HasValue)
        {
            orderId = firstOrderId.Value;
        }
        else
        {
            var fo = invoice.InvoiceOrders.FirstOrDefault(io => io.OrderId.HasValue)?.Order;
            orderId = fo?.Id ?? Guid.Empty;
        }

        var status = invoice.Status == InvoiceStatus.Paid ? "Paid" :
                    invoice.Status == InvoiceStatus.Overdue ? "Overdue" :
                    invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow && invoice.Status != InvoiceStatus.Paid ? "Overdue" :
                    "Pending";

        var billingTypeDisplay = invoice.BillingType switch
        {
            BillingType.PerLogo => "Per Logo",
            BillingType.Weekly => "Weekly",
            BillingType.Monthly => "Monthly",
            BillingType.Manual => "Manual",
            _ => "Per Logo"
        };

        string? ResolveTitle(InvoiceOrder io)
        {
            if (io.OrderId.HasValue && orderLineInfo != null &&
                orderLineInfo.TryGetValue(io.OrderId.Value, out var meta))
            {
                return meta.Title;
            }

            return io.Order?.Title;
        }

        DateTime? ResolveDate(InvoiceOrder io)
        {
            if (io.OrderId.HasValue && orderLineInfo != null &&
                orderLineInfo.TryGetValue(io.OrderId.Value, out var meta))
            {
                return meta.CreatedAt;
            }

            return io.Order?.CreatedAt;
        }

        string? ResolveItemCurrency(InvoiceOrder io)
        {
            if (!io.OrderId.HasValue)
            {
                return null;
            }

            if (orderLineInfo != null && orderLineInfo.TryGetValue(io.OrderId.Value, out var meta))
            {
                return string.IsNullOrWhiteSpace(meta.CurrencyCode) ? null : meta.CurrencyCode.Trim();
            }

            var fromNav = io.Order?.CurrencyCode;
            return string.IsNullOrWhiteSpace(fromNav) ? null : fromNav.Trim();
        }

        var items = invoice.InvoiceOrders.Select(io => new InvoiceItemDto
        {
            Id = io.Id,
            OrderId = io.OrderId,
            OrderTitle = ResolveTitle(io),
            OrderDate = ResolveDate(io),
            Description = io.Description,
            Amount = io.Amount,
            CurrencyCode = ResolveItemCurrency(io)
        }).ToList();

        var distinctLineCurrencies = items
            .Select(i => i.CurrencyCode)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.ToUpperInvariant())
            .Distinct()
            .ToList();
        string? invoiceCurrency = distinctLineCurrencies.Count == 1
            ? distinctLineCurrencies[0]
            : distinctLineCurrencies.FirstOrDefault();

        if (invoice.Client == null)
        {
            throw new InvalidOperationException("Invoice has no associated client.");
        }

        if (invoice.Client.User == null)
        {
            throw new InvalidOperationException("Client has no associated user.");
        }

        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            OrderId = orderId,
            OrderIds = orderIds,
            ClientId = invoice.Client.UserId,
            ClientName = $"{invoice.Client.User.FirstName} {invoice.Client.User.LastName}",
            Amount = invoice.Amount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            BillingType = invoice.BillingType,
            BillingTypeDisplay = billingTypeDisplay,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            PaidDate = invoice.PaidDate,
            Status = status,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes,
            BillingPeriod = invoice.BillingPeriod,
            Items = items,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            IsLocked = invoice.Status == InvoiceStatus.Paid,
            CurrencyCode = invoiceCurrency,
            ExchangeRate = invoice.ExchangeRate,
            ExchangeRateFetchedAt = invoice.ExchangeRateFetchedAt,
            ExchangeRateIsStale = invoice.ExchangeRateIsStale
        };
    }

    private static bool InvoiceInvolvesPkr(ClientProfile client, IEnumerable<LogoOrder> orders)
    {
        if (string.Equals(client.CurrencyCode, "PKR", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return orders.Any(o => string.Equals(o.CurrencyCode, "PKR", StringComparison.OrdinalIgnoreCase));
    }

    private async Task ApplyExchangeRateSnapshotAsync(Invoice invoice, ClientProfile client, IEnumerable<LogoOrder> orders)
    {
        if (!InvoiceInvolvesPkr(client, orders))
        {
            return;
        }

        var rateInfo = await _currencyService.GetRateInfoAsync().ConfigureAwait(false);
        invoice.ExchangeRate = rateInfo.Rate;
        invoice.ExchangeRateFetchedAt = rateInfo.FetchedAt;
        invoice.ExchangeRateIsStale = rateInfo.IsStale;
    }

    public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(Guid? userId = null, string? userRole = null)
    {
        await UpdateOverdueInvoicesAsync();

        var query = _context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        if (userRole == "Client" && userId.HasValue)
        {
            var client = await _context.ClientProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted);
            if (client != null)
            {
                query = query.Where(i => i.ClientId == client.Id);
            }
        }

        var totalInvoices = await query.CountAsync();
        var paidInvoices = await query.CountAsync(i => i.Status == InvoiceStatus.Paid);
        var overdueInvoices = await query.CountAsync(i => i.Status == InvoiceStatus.Overdue);
        var unpaidInvoices = await query.CountAsync(i => i.Status != InvoiceStatus.Paid);

        var totalAmount = await query.SumAsync(i => i.TotalAmount);
        var paidAmount = await query.Where(i => i.Status == InvoiceStatus.Paid).SumAsync(i => i.TotalAmount);
        var unpaidAmount = await query.Where(i => i.Status != InvoiceStatus.Paid).SumAsync(i => i.TotalAmount);
        var overdueAmount = await query.Where(i => i.Status == InvoiceStatus.Overdue).SumAsync(i => i.TotalAmount);

        var todayUtc = DateTime.UtcNow.Date;
        var weekStart = todayUtc.AddDays(-(int)todayUtc.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var weekQuery = query.Where(i => i.CreatedAt >= weekStart && i.CreatedAt < weekEnd);
        var weekTotal = await weekQuery.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
        var weekPaid = await weekQuery.Where(i => i.Status == InvoiceStatus.Paid).SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
        var weekPending = await weekQuery.Where(i => i.Status != InvoiceStatus.Paid).SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

        var monthQuery = query.Where(i => i.CreatedAt >= monthStart && i.CreatedAt < monthEnd);
        var monthTotal = await monthQuery.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
        var monthPaid = await monthQuery.Where(i => i.Status == InvoiceStatus.Paid).SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
        var monthPending = await monthQuery.Where(i => i.Status != InvoiceStatus.Paid).SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

        return new InvoiceStatisticsDto
        {
            TotalInvoices = totalInvoices,
            PaidInvoices = paidInvoices,
            DueInvoices = unpaidInvoices,
            OverdueInvoices = overdueInvoices,
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            DueAmount = unpaidAmount,
            OverdueAmount = overdueAmount,
            WeekSummary = new InvoicePeriodSummaryDto
            {
                TotalAmount = weekTotal,
                PaidAmount = weekPaid,
                PendingAmount = weekPending
            },
            MonthSummary = new InvoicePeriodSummaryDto
            {
                TotalAmount = monthTotal,
                PaidAmount = monthPaid,
                PendingAmount = monthPending
            }
        };
    }

    private static HashSet<Guid> CollectOrderIdsToMarkInvoiced(List<Guid> selectedOrderIds, List<CreateInvoiceItemDto>? manualItems)
    {
        var ids = selectedOrderIds.ToHashSet();
        if (manualItems == null)
        {
            return ids;
        }

        foreach (var manualItem in manualItems.Where(m => m.OrderId.HasValue))
        {
            ids.Add(manualItem.OrderId!.Value);
        }

        return ids;
    }

    private async Task ValidateOrdersEligibleForInvoicingAsync(List<LogoOrder> orders, Guid clientId)
    {
        var nonCompleted = orders.Where(o => o.Status != OrderStatus.Completed).ToList();
        if (nonCompleted.Any())
        {
            throw new InvalidOperationException("All orders must have status Completed before they can be added to an invoice.");
        }

        var alreadyInvoiced = orders.Where(o => o.IsInvoiced).ToList();
        if (alreadyInvoiced.Any())
        {
            throw new InvalidOperationException("One or more orders have already been invoiced.");
        }

        var notEligible = orders.Where(o => !o.BillingEligible).ToList();
        if (notEligible.Any())
        {
            throw new InvalidOperationException("One or more orders are not billing eligible.");
        }

        if (orders.Any(o => o.ClientId != clientId))
        {
            throw new InvalidOperationException("All orders must belong to the invoice client.");
        }
    }
}
