using AutoMapper;
using LogoDesignPortal.Application.DTOs.Reviews;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClientProfileEnsureService _clientProfileEnsure;

    public ReviewService(
        IApplicationDbContext context,
        IMapper mapper,
        IClientProfileEnsureService clientProfileEnsure)
    {
        _context = context;
        _mapper = mapper;
        _clientProfileEnsure = clientProfileEnsure;
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(CreateReviewRequestDto request, Guid clientId)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        var client = await _clientProfileEnsure.EnsureForClientUserAsync(clientId);

        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        // Check if review already exists
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == request.OrderId && r.ClientId == client.Id && !r.IsDeleted);

        if (existingReview != null)
        {
            throw new InvalidOperationException("Review already exists for this order.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            ClientId = client.Id,
            Rating = request.Rating,
            Comment = request.Comment,
            IsPublished = true,
            CreatedBy = clientId
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.Id) ?? throw new InvalidOperationException("Failed to create review.");
    }

    public async Task<List<ReviewResponseDto>> GetReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Client)
                .ThenInclude(c => c.User)
            .Where(r => !r.IsDeleted && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            ClientId = r.Client.UserId,
            ClientName = $"{r.Client.User.FirstName} {r.Client.User.LastName}",
            Rating = r.Rating,
            Comment = r.Comment,
            IsPublished = r.IsPublished,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<List<ReviewResponseDto>> GetReviewsByOrderAsync(Guid orderId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Client)
                .ThenInclude(c => c.User)
            .Where(r => r.OrderId == orderId && !r.IsDeleted && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            ClientId = r.Client.UserId,
            ClientName = $"{r.Client.User.FirstName} {r.Client.User.LastName}",
            Rating = r.Rating,
            Comment = r.Comment,
            IsPublished = r.IsPublished,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<ReviewResponseDto?> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

        if (review == null)
        {
            return null;
        }

        return new ReviewResponseDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            ClientId = review.Client.UserId,
            ClientName = $"{review.Client.User.FirstName} {review.Client.User.LastName}",
            Rating = review.Rating,
            Comment = review.Comment,
            IsPublished = review.IsPublished,
            CreatedAt = review.CreatedAt
        };
    }
}
