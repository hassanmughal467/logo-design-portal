using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Domain.Entities;

namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Ensures a <see cref="ClientProfile"/> exists for a user id when the account is a Client
/// (creates a minimal profile or restores a soft-deleted one). Used by orders, gallery, quotes, and reviews.
/// </summary>
public interface IClientProfileEnsureService
{
    /// <exception cref="UnauthorizedAccessException">The user id is not present or the account is inactive.</exception>
    /// <exception cref="ForbiddenAccessException">The user exists but is not in the Client role.</exception>
    Task<ClientProfile> EnsureForClientUserAsync(Guid userId, CancellationToken cancellationToken = default);
}