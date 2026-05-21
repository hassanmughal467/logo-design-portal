using LogoDesignPortal.Application.DTOs.Gallery;

namespace LogoDesignPortal.Application.Interfaces;

public interface IGalleryService
{
    Task<List<GalleryItemResponseDto>> GetClientGalleryAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<GalleryItemResponseDto?> GetGalleryItemByIdAsync(
        Guid galleryItemId,
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<bool> AddToGalleryAsync(Guid orderId, Guid fileId, Guid clientId, CancellationToken cancellationToken = default);
}
