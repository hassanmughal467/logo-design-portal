using LogoDesignPortal.Application.DTOs.DesignerLogoPricing;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Interfaces;

public interface IDesignerLogoPricingService
{
    Task<DesignerLogoPricingResponseDto> CreateAsync(CreateDesignerLogoPricingRequestDto request, Guid createdBy);
    Task<DesignerLogoPricingResponseDto?> GetByIdAsync(Guid id);
    Task<List<DesignerLogoPricingResponseDto>> GetByDesignerIdAsync(Guid designerId);
    Task<DesignerLogoPricingResponseDto> UpdateAsync(Guid id, UpdateDesignerLogoPricingRequestDto request, Guid updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid deletedBy);
    Task<DesignerLogoPricingResponseDto?> GetByDesignerAndDesignAsync(Guid designerId, int designCategory, int designType);

    /// <summary>
    /// Lookup designer default price for a design type. Returns DefaultPrice or null if not found/custom required.
    /// </summary>
    Task<decimal?> GetDesignerDefaultPriceAsync(Guid designerId, DesignCategory designCategory, DesignType designType);
}
