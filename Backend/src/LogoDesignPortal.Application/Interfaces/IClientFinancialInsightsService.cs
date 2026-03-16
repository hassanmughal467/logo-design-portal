using LogoDesignPortal.Application.DTOs.Client;

namespace LogoDesignPortal.Application.Interfaces;

/// <summary>
/// Generates financial insights for the logged-in client's dashboard.
/// </summary>
public interface IClientFinancialInsightsService
{
    Task<FinancialInsightsResponseDto> GetFinancialInsightsAsync(Guid clientUserId);
}
