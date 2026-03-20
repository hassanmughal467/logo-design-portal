using LogoDesignPortal.Application.DTOs.Roles;

namespace LogoDesignPortal.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
