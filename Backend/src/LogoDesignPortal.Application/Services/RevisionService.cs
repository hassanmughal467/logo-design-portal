using AutoMapper;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

public class RevisionService : IRevisionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RevisionService> _logger;
    private readonly string _fileStoragePath;
    private readonly string _temporaryStoragePath;
    private readonly string _permanentStoragePath;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public RevisionService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<RevisionService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _fileStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        _temporaryStoragePath = Path.Combine(_fileStoragePath, "Temporary");
        _permanentStoragePath = Path.Combine(_fileStoragePath, "Permanent");
        
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        try
        {
            if (!Directory.Exists(_fileStoragePath))
                Directory.CreateDirectory(_fileStoragePath);
            if (!Directory.Exists(_temporaryStoragePath))
                Directory.CreateDirectory(_temporaryStoragePath);
            if (!Directory.Exists(_permanentStoragePath))
                Directory.CreateDirectory(_permanentStoragePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot create file storage directories at {Path}. Revision uploads will fail until write permissions are granted to the IIS App Pool identity.", _fileStoragePath);
        }
    }

    public async Task<bool> CanRequestRevisionAsync(Guid orderId, Guid userId, string userRole)
    {
        if (userRole != "Client")
            return false;

        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null || order.Client.UserId != userId)
            return false;

        // Allow revision only when status is PreviewDelivered
        return order.Status == OrderStatus.PreviewDelivered;
    }

    public async Task<bool> CanApproveLogoAsync(Guid orderId, Guid userId, string userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            return false;

        // Allow approval when status is PreviewDelivered
        if (order.Status != OrderStatus.PreviewDelivered)
            return false;

        // Client can approve their own orders
        if (userRole == "Client")
        {
            return order.Client.UserId == userId;
        }

        // Admin and SuperAdmin can also approve
        if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            return true;
        }

        return false;
    }

    public async Task<RevisionResponseDto> RequestRevisionAsync(Guid orderId, RequestRevisionDto request, Guid requestedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (order.Client.UserId != requestedBy)
            throw new ForbiddenAccessException("You don't have permission to request revision for this order.");

        if (!await CanRequestRevisionAsync(orderId, requestedBy, "Client"))
            throw new InvalidOperationException("Revision can only be requested when order status is PreviewDelivered.");

        // Permanently delete all previous preview files
        await DeletePreviewFilesAsync(orderId);

        // Permanently delete all previous revision files
        await DeleteRevisionFilesAsync(orderId);

        // Archive previous revision instructions (soft delete, keep text)
        var previousRevisions = await _context.OrderRevisions
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .ToListAsync();

        foreach (var revision in previousRevisions)
        {
            revision.IsDeleted = true;
            revision.DeletedAt = DateTime.UtcNow;
            revision.DeletedBy = requestedBy;
        }

        // Create new revision
        var newRevision = new OrderRevision
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Instructions = request.Instructions,
            RequestedBy = requestedBy,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = requestedBy
        };

        _context.OrderRevisions.Add(newRevision);

        // Upload revision files if provided
        if (request.Files != null && request.Files.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".ai", ".eps", ".psd" };
            
            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                if (file.Length > MaxFileSize)
                    throw new InvalidOperationException($"File '{file.FileName}' exceeds maximum allowed size (10MB).");

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException($"File type '{fileExtension}' is not allowed for file '{file.FileName}'.");

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_temporaryStoragePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var revisionFile = new RevisionFile
                {
                    Id = Guid.NewGuid(),
                    RevisionId = newRevision.Id,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileType = RevisionFileType.ReferenceImage,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = requestedBy
                };

                _context.RevisionFiles.Add(revisionFile);
            }
        }

        // Update order status
        order.Status = OrderStatus.RevisionRequested;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = requestedBy;

        // Create audit log
        await CreateAuditLogAsync("RequestRevision", "Order", orderId, requestedBy, 
            $"Revision requested: {request.Instructions}");

        await _context.SaveChangesAsync();

        return await GetRevisionResponseAsync(newRevision.Id);
    }

    public async Task<RevisionResponseDto?> GetLatestRevisionAsync(Guid orderId, Guid? userId, string? userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            return null;

        // Authorization
        if (userRole == "Client" && order.Client.UserId != userId)
            throw new ForbiddenAccessException("You don't have access to this order's revisions.");

        if (userRole == "Designer")
        {
            if (order.DesignerId == null)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");

            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
                throw new ForbiddenAccessException("You don't have access to this order's revisions.");
        }

        // Only Admin and SuperAdmin can see revisions
        // Designers can see the latest revision for their assigned orders
        if (userRole != "Admin" && userRole != "SuperAdmin" && userRole != "Designer")
            return null;

        var latestRevision = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestRevision == null)
            return null;

        return await GetRevisionResponseAsync(latestRevision.Id);
    }

    public async Task<OrderResponseDto> ApproveLogoAsync(Guid orderId, ApproveLogoDto request, Guid approvedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        // Get the user to check their role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == approvedBy);

        var userRole = user?.Role?.Name ?? string.Empty;

        // Check permissions
        if (userRole == "Client")
        {
            if (order.Client.UserId != approvedBy)
                throw new ForbiddenAccessException("You don't have permission to approve this order.");
        }
        else if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            throw new ForbiddenAccessException("You don't have permission to approve this order.");
        }

        if (!await CanApproveLogoAsync(orderId, approvedBy, userRole))
            throw new InvalidOperationException("Logo can only be approved when order status is PreviewDelivered.");

        // Get all preview files (these will become final approved files)
        var previewFiles = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();

        // Get all revision files
        var revisionFiles = await _context.RevisionFiles
            .Include(rf => rf.Revision)
            .Where(rf => rf.Revision.OrderId == orderId && !rf.IsDeleted)
            .ToListAsync();

        // Permanently delete all revision files first
        foreach (var revisionFile in revisionFiles)
        {
            if (System.IO.File.Exists(revisionFile.FilePath))
            {
                System.IO.File.Delete(revisionFile.FilePath);
            }
            revisionFile.IsDeleted = true;
            revisionFile.DeletedAt = DateTime.UtcNow;
            revisionFile.DeletedBy = approvedBy;
        }

        // Convert preview files to final files and move to permanent storage
        foreach (var file in previewFiles)
        {
            // Move to permanent storage
            var permanentPath = Path.Combine(_permanentStoragePath, file.FileName);
            if (System.IO.File.Exists(file.FilePath))
            {
                // Ensure target directory exists
                var targetDir = Path.GetDirectoryName(permanentPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir!);
                }
                
                System.IO.File.Move(file.FilePath, permanentPath, overwrite: true);
                file.FilePath = permanentPath;
            }

            // Update file to Final type
            file.FileType = FileType.Final;
            file.IsFinalVersion = true;
            file.IsVisibleToClient = true;
            file.IsAdminApproved = true;
            file.ApprovedBy = approvedBy;
            file.ApprovedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;
            file.UpdatedBy = approvedBy;

            // Add to client gallery
            var galleryItem = new ClientGallery
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ClientId = order.ClientId,
                FileId = file.Id,
                FileName = file.FileName,
                OriginalFileName = file.OriginalFileName,
                FilePath = permanentPath,
                PreviewImagePath = permanentPath, // For now, use same path
                ContentType = file.ContentType,
                Format = Path.GetExtension(file.OriginalFileName).TrimStart('.'),
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = approvedBy
            };

            _context.ClientGalleries.Add(galleryItem);
        }

        // Update order status and disable uploads for completed orders
        order.Status = OrderStatus.Completed;
        order.AllowUploads = false; // Disable uploads when logo is final approved
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = approvedBy;

        // Create audit log
        await CreateAuditLogAsync("ApproveLogo", "Order", orderId, approvedBy, 
            $"Logo approved. Notes: {request.Notes ?? "None"}");

        await _context.SaveChangesAsync();

        // Return order response using mapper
        var updatedOrder = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        return _mapper.Map<OrderResponseDto>(updatedOrder);
    }

    private async Task DeletePreviewFilesAsync(Guid orderId)
    {
        var previewFiles = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();

        foreach (var file in previewFiles)
        {
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
        }

        _context.LogoFiles.RemoveRange(previewFiles);
    }

    private async Task DeleteRevisionFilesAsync(Guid orderId)
    {
        var revisions = await _context.OrderRevisions
            .Include(r => r.Files)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .ToListAsync();

        foreach (var revision in revisions)
        {
            foreach (var file in revision.Files)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }
            }
            _context.RevisionFiles.RemoveRange(revision.Files);
        }
    }

    private async Task<RevisionResponseDto> GetRevisionResponseAsync(Guid revisionId)
    {
        var revision = await _context.OrderRevisions
            .Include(r => r.Files.Where(f => !f.IsDeleted))
            .Include(r => r.Order)
                .ThenInclude(o => o.Client)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null)
            throw new InvalidOperationException("Revision not found.");

        var requestedByUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == revision.RequestedBy);

        return new RevisionResponseDto
        {
            Id = revision.Id,
            OrderId = revision.OrderId,
            Instructions = revision.Instructions,
            RequestedBy = revision.RequestedBy,
            RequestedByName = requestedByUser != null 
                ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" 
                : string.Empty,
            IsResolved = revision.IsResolved,
            ResolvedAt = revision.ResolvedAt,
            CreatedAt = revision.CreatedAt,
            Files = revision.Files.Select(f => new RevisionFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                OriginalFileName = f.OriginalFileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                CreatedAt = f.CreatedAt
            }).ToList()
        };
    }

    private async Task CreateAuditLogAsync(string action, string entityType, Guid entityId, Guid userId, string details)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedByUserId = userId,
            PerformedByRole = user?.Role?.Name,
            Notes = details,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.AuditLogs.Add(auditLog);
    }
}
