using LogoDesignPortal.Application.DTOs.ClientLogoPricing;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface IClientLogoPricingService
{
    Task<ClientLogoPricingResponseDto> CreateAsync(CreateClientLogoPricingRequestDto request, Guid createdBy);
    Task<ClientLogoPricingResponseDto?> GetByIdAsync(Guid id);
    Task<List<ClientLogoPricingResponseDto>> GetByClientIdAsync(Guid clientId);
    Task<ClientLogoPricingResponseDto> UpdateAsync(Guid id, UpdateClientLogoPricingRequestDto request, Guid updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid deletedBy);
    Task<ClientLogoPricingResponseDto?> GetByClientAndDesignAsync(Guid clientId, int designCategory, int designType);

    /// <summary>
    /// Lookup client pricing for order creation. Returns (Price, CurrencyCode) or null if not found.
    /// </summary>
    Task<(decimal Price, string CurrencyCode)?> GetClientPricingForOrderAsync(Guid clientId, DesignCategory designCategory, DesignType designType);
}
