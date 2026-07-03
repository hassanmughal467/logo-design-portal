using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Roles;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LogoDesignPortal.Application.Services;

public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _distributedCache;
    private readonly IReadModelCacheVersions _readModelCache;

    public RoleService(IApplicationDbContext context, IDistributedCache distributedCache, IReadModelCacheVersions readModelCache)
    {
        _context = context;
        _distributedCache = distributedCache;
        _readModelCache = readModelCache;
    }

    public async Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"ldp:cache:lookup:roles:e{_readModelCache.RolesEpoch}";
        var cached = await DistributedJsonCache.GetAsync<List<RoleResponseDto>>(_distributedCache, cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        var list = await _context.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync(cancellationToken);

        // Static lookup data; roles change rarely — bump RolesEpoch when role CRUD exists.
        await DistributedJsonCache.SetAsync(_distributedCache, cacheKey, list, TimeSpan.FromMinutes(10), cancellationToken).ConfigureAwait(false);

        return list;
    }

    public async Task<RoleResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
