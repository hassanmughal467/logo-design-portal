using AutoMapper;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
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
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        // Check if invoice already exists
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.OrderId == request.OrderId && !i.IsDeleted);

        if (existingInvoice != null)
        {
            throw new InvalidOperationException("Invoice already exists for this order.");
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{order.Id.ToString().Substring(0, 8).ToUpper()}",
            Amount = order.Price,
            TaxAmount = request.TaxAmount ?? 0,
            TotalAmount = order.Price + (request.TaxAmount ?? 0),
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedBy = createdBy
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return await GetInvoiceByIdAsync(invoice.Id) ?? throw new InvalidOperationException("Failed to create invoice.");
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return null;
        }

        var status = invoice.IsPaid ? "Paid" : 
                    invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow ? "Overdue" : 
                    "Unpaid";

        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            OrderId = invoice.OrderId,
            ClientId = invoice.Order.Client.UserId,
            ClientName = $"{invoice.Order.Client.User.FirstName} {invoice.Order.Client.User.LastName}",
            Amount = invoice.Amount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            PaidDate = invoice.PaidDate,
            Status = status,
            PaymentMethod = invoice.PaymentMethod,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt
        };
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesAsync(Guid? userId, string? userRole)
    {
        var query = _context.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        // Filter by role
        if (userRole == "Client" && userId.HasValue)
        {
            query = query.Where(i => i.Order.Client.UserId == userId.Value);
        }

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

        return invoices.Select(invoice =>
        {
            var status = invoice.IsPaid ? "Paid" : 
                        invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow ? "Overdue" : 
                        "Unpaid";

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                OrderId = invoice.OrderId,
                ClientId = invoice.Order.Client.UserId,
                ClientName = $"{invoice.Order.Client.User.FirstName} {invoice.Order.Client.User.LastName}",
                Amount = invoice.Amount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                PaidDate = invoice.PaidDate,
                Status = status,
                PaymentMethod = invoice.PaymentMethod,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt
            };
        }).ToList();
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
            .Include(i => i.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .Where(i => i.Order.ClientId == client.Id && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invoices.Select(invoice =>
        {
            var status = invoice.IsPaid ? "Paid" : 
                        invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow ? "Overdue" : 
                        "Unpaid";

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                OrderId = invoice.OrderId,
                ClientId = invoice.Order.Client.UserId,
                ClientName = $"{invoice.Order.Client.User.FirstName} {invoice.Order.Client.User.LastName}",
                Amount = invoice.Amount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                PaidDate = invoice.PaidDate,
                Status = status,
                PaymentMethod = invoice.PaymentMethod,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt
            };
        }).ToList();
    }

    public async Task<InvoiceResponseDto> MarkInvoiceAsPaidAsync(Guid invoiceId, string? paymentMethod)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        invoice.IsPaid = true;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.PaymentMethod = paymentMethod ?? invoice.PaymentMethod;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to update invoice.");
    }

    public async Task<bool> SendInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
        {
            return false;
        }

        // TODO: Implement email sending logic
        // For now, just return true
        return true;
    }
}
