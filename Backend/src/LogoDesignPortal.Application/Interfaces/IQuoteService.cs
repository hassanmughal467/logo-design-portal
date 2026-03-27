using LogoDesignPortal.Application.DTOs.Quotes;
using Microsoft.AspNetCore.Http;

namespace LogoDesignPortal.Application.Interfaces;

public interface IQuoteService
{
    Task<QuoteResponseDto> CreateQuoteAsync(CreateQuoteRequestDto request, IFormFile[] files, Guid clientUserId);
    Task<List<QuoteResponseDto>> GetQuotesAsync(Guid userId, string? role, string? status = null);
    Task<QuoteResponseDto?> GetQuoteByIdAsync(Guid quoteId, Guid userId, string? role);
    Task<QuoteResponseDto> RespondToQuoteAsync(Guid quoteId, RespondQuoteRequestDto request, Guid adminUserId);
    Task<QuoteResponseDto> RejectQuoteAsync(Guid quoteId, Guid clientUserId);
    Task<QuoteResponseDto> ConvertToOrderAsync(Guid quoteId, Guid clientUserId, Guid? existingOrderId = null);
}
