using AutoMapper;
using LogoDesignPortal.Application.Configuration;
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
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IApplicationDbContext context, IMapper mapper, INotificationService notificationService, IRealtimeEntityUpdateSender entityUpdateSender, IOptions<ProductionSafetyOptions> safetyOptions, ILogger<InvoiceService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
        _logger = logger;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy)
    {
        if (_safetyOptions.DisableBillingGeneration)
            throw new InvalidOperationException("Billing temporarily disabled by administrator.");

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
                        throw new InvalidOperationException($"Invoice item price for order {order.Id} cannot be negative.");
                    if (editablePrice > 10_000_000)
                        throw new InvalidOperationException($"Invoice item price for order {order.Id} exceeds maximum allowed (10,000,000).");
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

        // Add manual items
        if (request.ManualItems != null && request.ManualItems.Any())
        {
            foreach (var manualItem in request.ManualItems)
            {
                if (manualItem.Amount < 0)
                    throw new InvalidOperationException("Manual invoice item amount cannot be negative.");
                if (manualItem.Amount > 10_000_000)
                    throw new InvalidOperationException("Manual invoice item amount exceeds maximum allowed (10,000,000).");
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

        // Atomic transaction: invoice + items + order updates - all or nothing
        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            _context.Invoices.Add(invoice);

            foreach (var item in invoiceItems)
            {
                item.InvoiceId = invoice.Id;
                _context.InvoiceOrders.Add(item);
            }

            var ordersToUpdate = await _context.LogoOrders.Where(o => orderIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var order in ordersToUpdate)
            {
                order.IsInvoiced = true;
                order.InvoiceId = invoice.Id;
            }

            await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Created, createdBy, "Invoice created");
            await _context.SaveChangesAsync(ct);
        });

        // Structured logging for production monitoring
        _logger.LogInformation("InvoiceGenerated. InvoiceId={InvoiceId}, OrderIds={OrderIds}, UserId={UserId}, Timestamp={Timestamp}",
            invoice.Id, string.Join(",", orderIds), createdBy, DateTime.UtcNow);

        // Notify client: Invoice created (SignalR failure must not break operation)
        try
        {
            var firstOrderId = invoiceItems.FirstOrDefault(io => io.OrderId.HasValue)?.OrderId;
            var orderNumber = firstOrderId.HasValue ? NotificationFormatHelper.GetOrderNumber(firstOrderId.Value) : "N/A";
            var invoiceDisplayNumber = NotificationFormatHelper.GetInvoiceNumber(invoice.InvoiceNumber);
            var title = "Invoice Generated";
            var message = $"Invoice (#{invoiceDisplayNumber}) generated for order (#{orderNumber})";
            await _notificationService.CreateNotificationAsync(
                client.UserId,
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
                await _entityUpdateSender.SendInvoiceGeneratedAsync(item.OrderId!.Value, invoice.Id, client.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR/Notification failed for InvoiceGenerated. InvoiceId={InvoiceId}. Operation succeeded.", invoice.Id);
        }

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

        // Access control: Client can only access their own invoices; Admin/SuperAdmin can access all
        if (userRole == "Client" && (invoice.Client == null || invoice.Client.UserId != userId))
        {
            return null;
        }

        return MapToInvoiceResponseDto(invoice);
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole)
    {
        var query = _context.Invoices
            .Include(i => i.Client)
                .ThenInclude(c => c.User)
            .Include(i => i.InvoiceOrders)
                .ThenInclude(io => io.Order)
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        // Filter by role
        if (userRole == "Client" && userId.HasValue)
        {
            query = query.Where(i => i.Client.UserId == userId.Value);
        }

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

        // Batch update overdue status (single SaveChanges instead of per-invoice)
        await BatchUpdateOverdueInvoicesAsync(invoices);

        return invoices.Select(MapToInvoiceResponseDto).ToList();
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByClientAsync(Guid clientId)
    {
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
                .ThenInclude(io => io.Order)
            .Where(i => i.ClientId == client.Id && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        await BatchUpdateOverdueInvoicesAsync(invoices);

        return invoices.Select(MapToInvoiceResponseDto).ToList();
    }

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequestDto request, Guid updatedBy)
    {
        if (_safetyOptions.DisableInvoiceEditing)
            throw new InvalidOperationException("Invoice editing temporarily disabled by administrator.");

        var invoice = await _context.Invoices
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

        // Update invoice fields
        if (request.BillingType.HasValue)
        {
            invoice.BillingType = request.BillingType.Value;
        }

        if (request.TaxAmount.HasValue)
        {
            invoice.TaxAmount = request.TaxAmount.Value;
            invoice.TotalAmount = invoice.Amount + invoice.TaxAmount;
        }

        if (request.DueDate.HasValue)
        {
            invoice.DueDate = request.DueDate.Value;
            
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
                            throw new InvalidOperationException("Invoice item amount cannot be negative.");
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

        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = updatedBy;

        // Create audit log
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Updated, updatedBy, "Invoice updated");

        await _context.SaveChangesAsync();

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
                dto.PerformedByName = name;
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

        foreach (var invoice in overdueInvoices)
        {
            if (invoice.Status != InvoiceStatus.Overdue)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
                
                // Create audit log (system action)
                await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Overdue, null, "Invoice automatically marked as overdue");
            }
        }

        await _context.SaveChangesAsync();
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
        if (toUpdate.Count == 0) return;

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
            return;
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

    private InvoiceResponseDto MapToInvoiceResponseDto(Invoice invoice)
    {
        // Get all order IDs
        var orderIds = invoice.InvoiceOrders
            .Where(io => io.OrderId.HasValue)
            .Select(io => io.OrderId!.Value)
            .ToList();

        // Get first order for backward compatibility
        var firstOrder = invoice.InvoiceOrders.FirstOrDefault(io => io.OrderId.HasValue)?.Order;
        var orderId = firstOrder?.Id ?? Guid.Empty;

        // Calculate status (with auto-update for overdue)
        var status = invoice.Status == InvoiceStatus.Paid ? "Paid" :
                    invoice.Status == InvoiceStatus.Overdue ? "Overdue" :
                    invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow && invoice.Status != InvoiceStatus.Paid ? "Overdue" :
                    "Pending";

        // Map billing type
        var billingTypeDisplay = invoice.BillingType switch
        {
            BillingType.PerLogo => "Per Logo",
            BillingType.Weekly => "Weekly",
            BillingType.Monthly => "Monthly",
            BillingType.Manual => "Manual",
            _ => "Per Logo"
        };

        // Map invoice items
        var items = invoice.InvoiceOrders.Select(io => new InvoiceItemDto
        {
            Id = io.Id,
            OrderId = io.OrderId,
            OrderTitle = io.Order?.Title,
            Description = io.Description,
            Amount = io.Amount
        }).ToList();

        if (invoice.Client == null)
            throw new InvalidOperationException("Invoice has no associated client.");
        if (invoice.Client.User == null)
            throw new InvalidOperationException("Client has no associated user.");

        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            OrderId = orderId, // Backward compatibility
            OrderIds = orderIds, // New: all order IDs
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
            IsLocked = invoice.Status == InvoiceStatus.Paid
        };
    }

    public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(Guid? userId = null, string? userRole = null)
    {
        var query = _context.Invoices
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        // Filter by role
        if (userRole == "Client" && userId.HasValue)
        {
            var client = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted);
            if (client != null)
            {
                query = query.Where(i => i.ClientId == client.Id);
            }
        }

        var invoices = await query.ToListAsync();

        await BatchUpdateOverdueInvoicesAsync(invoices);

        // Re-query to get updated statuses
        invoices = await query.ToListAsync();

        var statistics = new InvoiceStatisticsDto
        {
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            DueInvoices = invoices.Count(i => i.Status != InvoiceStatus.Paid), // Unpaid = Pending + Overdue
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            TotalAmount = invoices.Sum(i => i.TotalAmount),
            PaidAmount = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
            DueAmount = invoices.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.TotalAmount), // Unpaid = Pending + Overdue
            OverdueAmount = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount)
        };

        return statistics;
    }
}
