using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

/// <summary>
/// Ensures a <see cref="ClientProfile"/> row exists for client users. Handles soft-deleted profiles,
/// minimal profile creation, and rare concurrent-create races without surfacing raw database errors to clients.
/// </summary>
public class ClientProfileEnsureService : IClientProfileEnsureService
{
    private const string DefaultCompanyLabel = "Personal";

    private readonly IApplicationDbContext _context;
    private readonly IReadModelCacheVersions _readModelCache;
    private readonly ILogger<ClientProfileEnsureService> _logger;

    public ClientProfileEnsureService(
        IApplicationDbContext context,
        IReadModelCacheVersions readModelCache,
        ILogger<ClientProfileEnsureService> logger)
    {
        _context = context;
        _readModelCache = readModelCache;
        _logger = logger;
    }

    private void BumpCaches(bool invalidateUsersToo)
    {
        _readModelCache.BumpOrders();
        _readModelCache.BumpAnalytics();
        if (invalidateUsersToo)
        {
            _readModelCache.BumpUsers();
        }
    }

    private static string DeriveDisplayName(User user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? DefaultCompanyLabel : name;
    }

    private static bool IsClientRole(User user) =>
        string.Equals(user.Role?.Name, "Client", StringComparison.OrdinalIgnoreCase);

    private static bool IsLikelyUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("2627", StringComparison.OrdinalIgnoreCase)
            || message.Contains("2601", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<ClientProfile?> TryLoadActiveProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

    /// <inheritdoc />
    public async Task<ClientProfile> EnsureForClientUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existing = await TryLoadActiveProfileAsync(userId, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Client profile ensure failed: user {UserId} not found or inactive.", userId);
            throw new UnauthorizedAccessException("Your session is no longer valid. Please sign in again.");
        }

        if (!IsClientRole(user))
        {
            _logger.LogWarning(
                "Client profile ensure failed: user {UserId} has role {Role}, expected Client.",
                userId, user.Role?.Name ?? "(null)");
            throw new ForbiddenAccessException("This feature is only available for client accounts.");
        }

        // Soft-deleted rows are excluded by global query filters; must bypass for restore.
        var deletedProfile = await _context.ClientProfiles
            .IgnoreQueryFilters()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDeleted, cancellationToken);

        if (deletedProfile != null)
        {
            deletedProfile.IsDeleted = false;
            deletedProfile.DeletedAt = null;
            deletedProfile.DeletedBy = null;
            var display = DeriveDisplayName(user);
            deletedProfile.CompanyName = string.IsNullOrWhiteSpace(deletedProfile.CompanyName)
                ? display
                : deletedProfile.CompanyName;
            if (string.IsNullOrWhiteSpace(deletedProfile.CompanyName))
            {
                deletedProfile.CompanyName = DefaultCompanyLabel;
            }

            deletedProfile.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsLikelyUniqueConstraintViolation(ex))
            {
                _context.Untrack(deletedProfile);
                var raced = await TryLoadActiveProfileAsync(userId, cancellationToken);
                if (raced != null)
                {
                    _logger.LogInformation(
                        ex,
                        "Resolved concurrent client profile restore for UserId={UserId} by loading existing row.",
                        userId);
                    BumpCaches(invalidateUsersToo: true);
                    return raced;
                }
                throw;
            }

            _logger.LogInformation("Restored soft-deleted ClientProfile for UserId={UserId}", userId);
            BumpCaches(invalidateUsersToo: true);
            return deletedProfile;
        }

        var companyName = DeriveDisplayName(user);
        var newProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CurrencyCode = ClientCurrencyHelper.DefaultCode,
            CompanyName = companyName,
            ContactName = companyName,
            CreatedAt = DateTime.UtcNow
        };
        if (string.IsNullOrWhiteSpace(newProfile.CompanyName))
        {
            newProfile.CompanyName = DefaultCompanyLabel;
        }

        _context.ClientProfiles.Add(newProfile);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsLikelyUniqueConstraintViolation(ex))
        {
            _context.Untrack(newProfile);
            var raced = await TryLoadActiveProfileAsync(userId, cancellationToken);
            if (raced != null)
            {
                _logger.LogInformation(
                    ex,
                    "Resolved concurrent client profile creation for UserId={UserId} by loading existing row.",
                    userId);
                BumpCaches(invalidateUsersToo: true);
                return raced;
            }
            _logger.LogError(ex, "Client profile creation failed for UserId={UserId}", userId);
            throw;
        }

        _logger.LogInformation("Auto-created ClientProfile for UserId={UserId}", userId);
        BumpCaches(invalidateUsersToo: true);

        // Re-load with includes so navigations match callers (e.g. notifications using User).
        return await _context.ClientProfiles
            .Include(c => c.User)
            .FirstAsync(c => c.Id == newProfile.Id, cancellationToken);
    }
}
