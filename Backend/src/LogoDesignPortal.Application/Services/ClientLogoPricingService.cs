using LogoDesignPortal.Application.DTOs.ClientLogoPricing;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class ClientLogoPricingService : IClientLogoPricingService
{
    private readonly IApplicationDbContext _context;

    public ClientLogoPricingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientLogoPricingResponseDto> CreateAsync(CreateClientLogoPricingRequestDto request, Guid createdBy)
    {
        // Check for existing (including soft-deleted) - we'll replace by updating if exists
        var existing = await _context.ClientLogoPricings
            .FirstOrDefaultAsync(p => p.ClientId == request.ClientId
                && p.DesignCategory == request.DesignCategory
                && p.DesignType == request.DesignType
                && !p.IsDeleted);

        if (existing != null)
        {
            existing.Price = request.Price;
            existing.CurrencyCode = request.CurrencyCode;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = createdBy;
            await _context.SaveChangesAsync();
            return await MapToDtoAsync(existing);
        }

        var entity = new ClientLogoPricing
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            DesignCategory = request.DesignCategory,
            DesignType = request.DesignType,
            Price = request.Price,
            CurrencyCode = request.CurrencyCode,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.ClientLogoPricings.Add(entity);
        await _context.SaveChangesAsync();
        return await MapToDtoAsync(entity);
    }

    public async Task<ClientLogoPricingResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.ClientLogoPricings
            .Include(p => p.Client)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        return entity == null ? null : await MapToDtoAsync(entity);
    }

    public async Task<List<ClientLogoPricingResponseDto>> GetByClientIdAsync(Guid clientId)
    {
        var entities = await _context.ClientLogoPricings
            .Include(p => p.Client)
            .ThenInclude(c => c.User)
            .Where(p => p.ClientId == clientId && !p.IsDeleted)
            .OrderBy(p => p.DesignCategory)
            .ThenBy(p => p.DesignType)
            .ToListAsync();

        var result = new List<ClientLogoPricingResponseDto>();
        foreach (var e in entities)
        {
            result.Add(await MapToDtoAsync(e));
        }
        return result;
    }

    public async Task<ClientLogoPricingResponseDto> UpdateAsync(Guid id, UpdateClientLogoPricingRequestDto request, Guid updatedBy)
    {
        var entity = await _context.ClientLogoPricings
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new InvalidOperationException("Client logo pricing not found.");

        entity.Price = request.Price;
        entity.CurrencyCode = request.CurrencyCode;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
    {
        var entity = await _context.ClientLogoPricings
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ClientLogoPricingResponseDto?> GetByClientAndDesignAsync(Guid clientId, int designCategory, int designType)
    {
        if (!Enum.IsDefined(typeof(DesignCategory), designCategory) || !Enum.IsDefined(typeof(DesignType), designType))
            return null;

        var entity = await _context.ClientLogoPricings
            .FirstOrDefaultAsync(p =>
                p.ClientId == clientId
                && (int)p.DesignCategory == designCategory
                && (int)p.DesignType == designType
                && p.IsActive
                && !p.IsDeleted);

        return entity == null ? null : await MapToDtoAsync(entity);
    }

    /// <summary>
    /// Lookup client pricing for order creation. Returns (Price, CurrencyCode) or null if not found.
    /// </summary>
    public async Task<(decimal Price, string CurrencyCode)?> GetClientPricingForOrderAsync(Guid clientId, DesignCategory designCategory, DesignType designType)
    {
        var entity = await _context.ClientLogoPricings
            .FirstOrDefaultAsync(p =>
                p.ClientId == clientId
                && p.DesignCategory == designCategory
                && p.DesignType == designType
                && p.IsActive
                && !p.IsDeleted);

        return entity == null ? null : (entity.Price, entity.CurrencyCode);
    }

    private async Task<ClientLogoPricingResponseDto> MapToDtoAsync(ClientLogoPricing entity)
    {
        string? clientName = null;
        if (entity.Client?.User != null)
        {
            clientName = $"{entity.Client.User.FirstName} {entity.Client.User.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName))
                clientName = entity.Client.CompanyName;
        }
        else if (entity.ClientId != Guid.Empty)
        {
            var client = await _context.ClientProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == entity.ClientId);
            if (client != null)
                clientName = $"{client.User.FirstName} {client.User.LastName}".Trim()
                    ?? client.CompanyName;
        }

        return new ClientLogoPricingResponseDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            ClientName = clientName,
            DesignCategory = (int)entity.DesignCategory,
            DesignCategoryName = entity.DesignCategory.ToString(),
            DesignType = (int)entity.DesignType,
            DesignTypeName = entity.DesignType.ToString(),
            Price = entity.Price,
            CurrencyCode = entity.CurrencyCode,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
