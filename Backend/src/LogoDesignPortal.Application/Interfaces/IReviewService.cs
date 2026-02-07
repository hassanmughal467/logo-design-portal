using LogoDesignPortal.Application.DTOs.Reviews;

namespace LogoDesignPortal.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewResponseDto> CreateReviewAsync(CreateReviewRequestDto request, Guid clientId);
    Task<List<ReviewResponseDto>> GetReviewsAsync();
    Task<List<ReviewResponseDto>> GetReviewsByOrderAsync(Guid orderId);
    Task<ReviewResponseDto?> GetReviewByIdAsync(Guid reviewId);
}
