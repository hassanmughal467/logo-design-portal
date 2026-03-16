using LogoDesignPortal.Application.DTOs.DesignerLogoPricing;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class DesignerLogoPricingService : IDesignerLogoPricingService
{
    private readonly IApplicationDbContext _context;

    public DesignerLogoPricingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DesignerLogoPricingResponseDto> CreateAsync(CreateDesignerLogoPricingRequestDto request, Guid createdBy)
    {
        var existing = await _context.DesignerLogoPricings
            .FirstOrDefaultAsync(p => p.DesignerId == request.DesignerId
                && p.DesignCategory == request.DesignCategory
                && p.DesignType == request.DesignType
                && !p.IsDeleted);

        if (existing != null)
        {
            existing.DefaultPrice = request.DefaultPrice;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = createdBy;
            await _context.SaveChangesAsync();
            return await MapToDtoAsync(existing);
        }

        var entity = new DesignerLogoPricing
        {
            Id = Guid.NewGuid(),
            DesignerId = request.DesignerId,
            DesignCategory = request.DesignCategory,
            DesignType = request.DesignType,
            DefaultPrice = request.DefaultPrice,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.DesignerLogoPricings.Add(entity);
        await _context.SaveChangesAsync();
        return await MapToDtoAsync(entity);
    }

    public async Task<DesignerLogoPricingResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.DesignerLogoPricings
            .Include(p => p.Designer)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        return entity == null ? null : await MapToDtoAsync(entity);
    }

    public async Task<List<DesignerLogoPricingResponseDto>> GetByDesignerIdAsync(Guid designerId)
    {
        var entities = await _context.DesignerLogoPricings
            .Include(p => p.Designer)
            .ThenInclude(d => d.User)
            .Where(p => p.DesignerId == designerId && !p.IsDeleted)
            .OrderBy(p => p.DesignCategory)
            .ThenBy(p => p.DesignType)
            .ToListAsync();

        var result = new List<DesignerLogoPricingResponseDto>();
        foreach (var e in entities)
        {
            result.Add(await MapToDtoAsync(e));
        }
        return result;
    }

    public async Task<DesignerLogoPricingResponseDto> UpdateAsync(Guid id, UpdateDesignerLogoPricingRequestDto request, Guid updatedBy)
    {
        var entity = await _context.DesignerLogoPricings
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new InvalidOperationException("Designer logo pricing not found.");

        entity.DefaultPrice = request.DefaultPrice;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
    {
        var entity = await _context.DesignerLogoPricings
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

    public async Task<DesignerLogoPricingResponseDto?> GetByDesignerAndDesignAsync(Guid designerId, int designCategory, int designType)
    {
        if (!Enum.IsDefined(typeof(DesignCategory), designCategory) || !Enum.IsDefined(typeof(DesignType), designType))
            return null;

        var entity = await _context.DesignerLogoPricings
            .FirstOrDefaultAsync(p =>
                p.DesignerId == designerId
                && (int)p.DesignCategory == designCategory
                && (int)p.DesignType == designType
                && p.IsActive
                && !p.IsDeleted);

        return entity == null ? null : await MapToDtoAsync(entity);
    }

    public async Task<decimal?> GetDesignerDefaultPriceAsync(Guid designerId, DesignCategory designCategory, DesignType designType)
    {
        var entity = await _context.DesignerLogoPricings
            .FirstOrDefaultAsync(p =>
                p.DesignerId == designerId
                && p.DesignCategory == designCategory
                && p.DesignType == designType
                && p.IsActive
                && !p.IsDeleted);

        if (entity == null || entity.DefaultPrice <= 0)
            return null;
        return entity.DefaultPrice;
    }

    private async Task<DesignerLogoPricingResponseDto> MapToDtoAsync(DesignerLogoPricing entity)
    {
        string? designerName = null;
        var user = entity.Designer?.User;
        if (user != null)
        {
            designerName = $"{user.FirstName} {user.LastName}".Trim();
        }
        else if (entity.DesignerId != Guid.Empty)
        {
            var designer = await _context.DesignerProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == entity.DesignerId);
            if (designer?.User != null)
                designerName = $"{designer.User.FirstName} {designer.User.LastName}".Trim();
        }

        return new DesignerLogoPricingResponseDto
        {
            Id = entity.Id,
            DesignerId = entity.DesignerId,
            DesignerName = designerName,
            DesignCategory = (int)entity.DesignCategory,
            DesignCategoryName = entity.DesignCategory.ToString(),
            DesignType = (int)entity.DesignType,
            DesignTypeName = entity.DesignType.ToString(),
            DefaultPrice = entity.DefaultPrice,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
