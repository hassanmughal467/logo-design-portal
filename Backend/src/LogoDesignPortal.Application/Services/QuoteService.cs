using System.Text.Json;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Quotes;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.Application.Services;

public class QuoteService : IQuoteService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IOrderService _orderService;
    private readonly string _quoteStoragePath;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".svg", ".ai", ".eps", ".psd" };
    private const long ImageMaxBytes = 10 * 1024 * 1024;
    private const long DocumentMaxBytes = 25 * 1024 * 1024;

    public QuoteService(
        IApplicationDbContext context,
        INotificationService notificationService,
        IOrderService orderService,
        IConfiguration configuration)
    {
        _context = context;
        _notificationService = notificationService;
        _orderService = orderService;
        var root = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        _quoteStoragePath = Path.Combine(root, "Quotes");
        Directory.CreateDirectory(_quoteStoragePath);
    }

    public async Task<QuoteResponseDto> CreateQuoteAsync(CreateQuoteRequestDto request, IFormFile[] files, Guid clientUserId)
    {
        var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId && !c.IsDeleted)
            ?? throw new InvalidOperationException("Client profile not found.");

        var savedFiles = await SaveQuoteAttachmentsAsync(files);
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            LogoName = request.LogoName.Trim(),
            Description = request.Description.Trim(),
            RequestedBudget = request.RequestedBudget,
            AttachmentsJson = JsonSerializer.Serialize(savedFiles),
            Status = QuoteStatus.Pending,
            CreatedBy = clientUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationForRoleAsync(
            "Admin",
            "New Quote Request",
            $"A new quote request was submitted for '{quote.LogoName}'.",
            NotificationType.Info,
            NotificationReferenceType.Quote,
            quote.Id);

        await _notificationService.CreateNotificationForRoleAsync(
            "SuperAdmin",
            "New Quote Request",
            $"A new quote request was submitted for '{quote.LogoName}'.",
            NotificationType.Info,
            NotificationReferenceType.Quote,
            quote.Id);

        return ToDto(quote);
    }

    public async Task<List<QuoteResponseDto>> GetQuotesAsync(Guid userId, string? role, string? status = null)
    {
        var query = _context.Quotes.AsQueryable();

        if (role == "Client")
        {
            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (client == null) return new List<QuoteResponseDto>();
            query = query.Where(q => q.ClientId == client.Id);
        }
        else if (role != "Admin" && role != "SuperAdmin")
        {
            return new List<QuoteResponseDto>();
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<QuoteStatus>(status, true, out var parsed))
        {
            query = query.Where(q => q.Status == parsed);
        }

        var quotes = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        return quotes.Select(ToDto).ToList();
    }

    public async Task<QuoteResponseDto?> GetQuoteByIdAsync(Guid quoteId, Guid userId, string? role)
    {
        var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote == null) return null;

        if (role == "Client")
        {
            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (client == null || client.Id != quote.ClientId)
                throw new UnauthorizedAccessException("You can only access your own quotes.");
        }
        else if (role != "Admin" && role != "SuperAdmin")
        {
            throw new UnauthorizedAccessException("You are not authorized to view this quote.");
        }

        return ToDto(quote);
    }

    public async Task<QuoteResponseDto> RespondToQuoteAsync(Guid quoteId, RespondQuoteRequestDto request, Guid adminUserId)
    {
        var quote = await _context.Quotes
            .Include(q => q.Client)
            .FirstOrDefaultAsync(q => q.Id == quoteId)
            ?? throw new InvalidOperationException("Quote not found.");

        if (quote.Status is QuoteStatus.Converted or QuoteStatus.Rejected)
            throw new InvalidOperationException("This quote can no longer be responded to.");

        quote.AdminQuotedPrice = request.AdminQuotedPrice;
        quote.AdminNotes = request.AdminNotes?.Trim();
        quote.Status = QuoteStatus.Responded;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            quote.Client.UserId,
            "Quote Response Received",
            $"Your quote '{quote.LogoName}' has received a response.",
            NotificationType.Info,
            null,
            NotificationReferenceType.Quote,
            quote.Id,
            adminUserId);

        return ToDto(quote);
    }

    public async Task<QuoteResponseDto> RejectQuoteAsync(Guid quoteId, Guid clientUserId)
    {
        var quote = await _context.Quotes.Include(q => q.Client).FirstOrDefaultAsync(q => q.Id == quoteId)
            ?? throw new InvalidOperationException("Quote not found.");

        if (quote.Client.UserId != clientUserId)
            throw new UnauthorizedAccessException("You can only reject your own quote.");

        if (quote.Status == QuoteStatus.Converted)
            throw new InvalidOperationException("Converted quotes cannot be rejected.");

        quote.Status = QuoteStatus.Rejected;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.UpdatedBy = clientUserId;
        await _context.SaveChangesAsync();

        return ToDto(quote);
    }

    public async Task<QuoteResponseDto> ConvertToOrderAsync(Guid quoteId, Guid clientUserId, Guid? existingOrderId = null)
    {
        var quote = await _context.Quotes.Include(q => q.Client).FirstOrDefaultAsync(q => q.Id == quoteId)
            ?? throw new InvalidOperationException("Quote not found.");

        if (quote.Client.UserId != clientUserId)
            throw new UnauthorizedAccessException("You can only convert your own quote.");

        if (quote.Status != QuoteStatus.Responded && quote.Status != QuoteStatus.Accepted)
            throw new InvalidOperationException("Only responded quotes can be converted.");

        if (quote.AdminQuotedPrice is null)
            throw new InvalidOperationException("Quote must have an admin quoted price before conversion.");

        LogoOrder persistedOrder;
        if (existingOrderId.HasValue)
        {
            persistedOrder = await _context.LogoOrders
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.Id == existingOrderId.Value && !o.IsDeleted)
                ?? throw new InvalidOperationException("Order not found for conversion.");

            if (persistedOrder.Client.UserId != clientUserId)
                throw new UnauthorizedAccessException("You can only link quotes to your own orders.");
        }
        else
        {
            var order = await _orderService.CreateOrderAsync(new CreateOrderRequestDto
            {
                Title = quote.LogoName,
                Description = quote.Description,
                Price = quote.AdminQuotedPrice.Value
            }, clientUserId);
            persistedOrder = await _context.LogoOrders.FirstAsync(o => o.Id == order.Id);
        }

        persistedOrder.OrderSource = OrderSource.Quote;
        persistedOrder.QuoteId = quote.Id;
        persistedOrder.Price = quote.AdminQuotedPrice.Value;
        persistedOrder.ClientChargePrice = quote.AdminQuotedPrice.Value;
        persistedOrder.ClientPrice = quote.AdminQuotedPrice.Value;
        persistedOrder.UpdatedAt = DateTime.UtcNow;
        persistedOrder.UpdatedBy = clientUserId;

        quote.Status = QuoteStatus.Converted;
        quote.ConvertedOrderId = persistedOrder.Id;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.UpdatedBy = clientUserId;

        await _context.SaveChangesAsync();
        return ToDto(quote);
    }

    private async Task<List<string>> SaveQuoteAttachmentsAsync(IFormFile[] files)
    {
        ValidateAttachments(files);
        var saved = new List<string>();
        foreach (var file in files)
        {
            if (file.Length <= 0) continue;
            var extension = Path.GetExtension(file.FileName);
            var safeName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(_quoteStoragePath, safeName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            saved.Add(safeName);
        }

        return saved;
    }

    private static void ValidateAttachments(IFormFile[] files)
    {
        if (files.Length > 10)
            throw new InvalidOperationException("A maximum of 10 attachments is allowed per quote.");

        foreach (var file in files)
        {
            if (file.Length <= 0)
                throw new InvalidOperationException("One or more attachments are empty.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Unsupported attachment type: {extension}");

            var maxSize = extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" ? ImageMaxBytes : DocumentMaxBytes;
            if (file.Length > maxSize)
                throw new InvalidOperationException($"Attachment '{file.FileName}' exceeds allowed size.");
        }
    }

    private static QuoteResponseDto ToDto(Quote quote)
    {
        List<string> attachments = new();
        if (!string.IsNullOrWhiteSpace(quote.AttachmentsJson))
        {
            attachments = JsonSerializer.Deserialize<List<string>>(quote.AttachmentsJson) ?? new List<string>();
        }

        return new QuoteResponseDto
        {
            Id = quote.Id,
            ClientId = quote.ClientId,
            LogoName = quote.LogoName,
            Description = quote.Description,
            RequestedBudget = quote.RequestedBudget,
            AdminQuotedPrice = quote.AdminQuotedPrice,
            AdminNotes = quote.AdminNotes,
            Status = quote.Status.ToString(),
            CreatedAt = quote.CreatedAt,
            Attachments = attachments,
            ConvertedOrderId = quote.ConvertedOrderId
        };
    }
}
