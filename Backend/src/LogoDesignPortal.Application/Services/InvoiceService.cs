using AutoMapper;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public InvoiceService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request, Guid createdBy)
    {
        // Determine which orders to include
        var orderIds = new List<Guid>();
        
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

            // Ensure all orders belong to the same client
            var firstClientId = orders.First().ClientId;
            if (orders.Any(o => o.ClientId != firstClientId))
            {
                throw new InvalidOperationException("All orders must belong to the same client.");
            }

            clientId = firstClientId;

            // Check if invoice already exists for any of these orders
            var existingInvoices = await _context.Invoices
                .Include(i => i.InvoiceOrders)
                .Where(i => !i.IsDeleted && i.InvoiceOrders.Any(io => orderIds.Contains(io.OrderId!.Value)))
                .ToListAsync();

            if (existingInvoices.Any())
            {
                throw new InvalidOperationException("Invoice already exists for one or more of these orders.");
            }
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
                var item = new InvoiceOrder
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = Guid.Empty, // Will be set after invoice creation
                    OrderId = order.Id,
                    Description = order.Title ?? $"Logo Design - Order #{order.Id.ToString().Substring(0, 8)}",
                    Amount = order.Price,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                invoiceItems.Add(item);
                totalAmount += order.Price;
            }
        }

        // Add manual items
        if (request.ManualItems != null && request.ManualItems.Any())
        {
            foreach (var manualItem in request.ManualItems)
            {
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
        var billingType = request.BillingType ?? BillingType.PerLogo; // Default for backward compatibility
        var taxAmount = request.TaxAmount ?? 0;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            InvoiceNumber = invoiceNumber,
            Amount = totalAmount, // Calculated from items
            TaxAmount = taxAmount,
            TotalAmount = totalAmount + taxAmount, // Fixed: Include tax in total amount
            Status = InvoiceStatus.Pending,
            BillingType = billingType,
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);

        // Set invoice ID for items and add them
        foreach (var item in invoiceItems)
        {
            item.InvoiceId = invoice.Id;
            _context.InvoiceOrders.Add(item);
        }

        // Create audit log
        await CreateInvoiceLogAsync(invoice.Id, InvoiceAction.Created, createdBy, "Invoice created");

        await _context.SaveChangesAsync();

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

        // Update status for overdue invoices
        foreach (var invoice in invoices)
        {
            await UpdateInvoiceStatusIfNeededAsync(invoice);
        }

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

        // Update status for overdue invoices
        foreach (var invoice in invoices)
        {
            await UpdateInvoiceStatusIfNeededAsync(invoice);
        }

        return invoices.Select(MapToInvoiceResponseDto).ToList();
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

            // Try to get performer name if available
            if (log.PerformedBy.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == log.PerformedBy.Value);
                if (user != null)
                {
                    dto.PerformedByName = $"{user.FirstName} {user.LastName}";
                }
            }

            return dto;
        }).ToList();
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

    private async Task UpdateInvoiceStatusIfNeededAsync(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Paid)
        {
            return; // Don't update paid invoices
        }

        if (invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow && invoice.Status != InvoiceStatus.Overdue)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;
            
            // Create audit log (system action)
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

        // Update status for overdue invoices
        foreach (var invoice in invoices)
        {
            await UpdateInvoiceStatusIfNeededAsync(invoice);
        }

        // Re-query to get updated statuses
        invoices = await query.ToListAsync();

        var statistics = new InvoiceStatisticsDto
        {
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            DueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Pending),
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            TotalAmount = invoices.Sum(i => i.TotalAmount),
            PaidAmount = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
            DueAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.TotalAmount),
            OverdueAmount = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount)
        };

        return statistics;
    }
}
