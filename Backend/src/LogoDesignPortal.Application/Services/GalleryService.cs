using AutoMapper;
using LogoDesignPortal.Application.DTOs.Gallery;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class GalleryService : IGalleryService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GalleryService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<GalleryItemResponseDto>> GetClientGalleryAsync(Guid clientId)
    {
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            throw new InvalidOperationException("Client profile not found.");
        }

        var galleryItems = await _context.ClientGalleries
            .Include(g => g.Order)
            .Include(g => g.File)
            .Where(g => g.ClientId == client.Id && !g.IsDeleted)
            .OrderByDescending(g => g.ApprovedAt)
            .ToListAsync();

        return _mapper.Map<List<GalleryItemResponseDto>>(galleryItems);
    }

    public async Task<GalleryItemResponseDto?> GetGalleryItemByIdAsync(Guid galleryItemId, Guid clientId)
    {
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            throw new InvalidOperationException("Client profile not found.");
        }

        var galleryItem = await _context.ClientGalleries
            .Include(g => g.Order)
            .Include(g => g.File)
            .FirstOrDefaultAsync(g => g.Id == galleryItemId && g.ClientId == client.Id && !g.IsDeleted);

        return galleryItem == null ? null : _mapper.Map<GalleryItemResponseDto>(galleryItem);
    }

    public async Task<bool> AddToGalleryAsync(Guid orderId, Guid fileId, Guid clientId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null || order.Client.UserId != clientId)
        {
            throw new InvalidOperationException("Order not found or access denied.");
        }

        var file = await _context.LogoFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.OrderId == orderId && !f.IsDeleted);

        if (file == null || !file.IsFinalVersion || !file.IsAdminApproved)
        {
            throw new InvalidOperationException("File not found or not approved for gallery.");
        }

        // Check if already in gallery
        var existing = await _context.ClientGalleries
            .FirstOrDefaultAsync(g => g.FileId == fileId && g.ClientId == order.ClientId && !g.IsDeleted);

        if (existing != null)
        {
            return true; // Already in gallery
        }

        // Generate preview image path (simplified - in production, generate JPEG preview)
        var previewPath = file.FilePath; // TODO: Generate JPEG preview

        var galleryItem = new ClientGallery
        {
            Id = Guid.NewGuid(),
            ClientId = order.ClientId,
            OrderId = orderId,
            FileId = fileId,
            PreviewImagePath = previewPath,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            FilePath = file.FilePath,
            ContentType = file.ContentType,
            Format = GetFileFormat(file.OriginalFileName),
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClientGalleries.Add(galleryItem);
        await _context.SaveChangesAsync();

        return true;
    }

    private string? GetFileFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.TrimStart('.').ToUpper();
        return extension;
    }
}
