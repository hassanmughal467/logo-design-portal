using LogoDesignPortal.Application.DTOs.Gallery;

namespace LogoDesignPortal.Application.Interfaces;

public interface IGalleryService
{
    Task<List<GalleryItemResponseDto>> GetClientGalleryAsync(Guid clientId);
    Task<GalleryItemResponseDto?> GetGalleryItemByIdAsync(Guid galleryItemId, Guid clientId);
    Task<bool> AddToGalleryAsync(Guid orderId, Guid fileId, Guid clientId);
}
