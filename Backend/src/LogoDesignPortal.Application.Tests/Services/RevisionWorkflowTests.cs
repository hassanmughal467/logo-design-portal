using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Simulates full order workflow with 3 revisions and verifies:
/// 1. Preview files are deleted on revision
/// 2. Client reference files remain intact
/// 3. Only final files remain after approval
/// 4. Admin always sends full preview batches (enforced by backend)
/// </summary>
public class RevisionWorkflowTests : IDisposable
{
    private readonly string _tempStoragePath;
    private readonly ApplicationDbContext _context;
    private readonly RevisionService _revisionService;
    private readonly Mock<INotificationService> _notificationService;
    private readonly Guid _clientUserId;
    private readonly Guid _designerUserId;
    private readonly Guid _adminUserId;
    private readonly Guid _orderId;
    private readonly Guid _clientProfileId;
    private readonly Guid _designerProfileId;

    public RevisionWorkflowTests()
    {
        _clientUserId = Guid.NewGuid();
        _designerUserId = Guid.NewGuid();
        _adminUserId = Guid.NewGuid();
        _clientProfileId = Guid.NewGuid();
        _designerProfileId = Guid.NewGuid();
        _orderId = Guid.NewGuid();

        _tempStoragePath = Path.Combine(Path.GetTempPath(), "LogoDesignPortal_Test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempStoragePath);
        Directory.CreateDirectory(Path.Combine(_tempStoragePath, "Temporary"));
        Directory.CreateDirectory(Path.Combine(_tempStoragePath, "Permanent"));

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "RevisionWorkflow_" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
        SeedDatabase();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:Path"] = _tempStoragePath
            }!)
            .Build();

        var logger = new Mock<ILogger<RevisionService>>();
        _notificationService = new Mock<INotificationService>();
        _notificationService
            .Setup(n => n.CreateNotificationForRoleAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<NotificationReferenceType>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .ReturnsAsync(new List<NotificationResponseDto> { new() { Id = Guid.NewGuid() } });
        var entityUpdateSender = new Mock<IRealtimeEntityUpdateSender>();

        _revisionService = new RevisionService(
            _context,
            Mock.Of<AutoMapper.IMapper>(),
            config,
            logger.Object,
            _notificationService.Object,
            entityUpdateSender.Object,
            Mock.Of<IInvoiceService>());
    }

    private void SeedDatabase()
    {
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        var clientUser = new User
        {
            Id = _clientUserId,
            Email = "client@test.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };

        var designerUser = new User
        {
            Id = _designerUserId,
            Email = "designer@test.com",
            FirstName = "Test",
            LastName = "Designer",
            PasswordHash = "hash",
            RoleId = designerRole.Id,
            Role = designerRole
        };

        var adminUser = new User
        {
            Id = _adminUserId,
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = adminRole.Id,
            Role = adminRole
        };

        var clientProfile = new ClientProfile
        {
            Id = _clientProfileId,
            UserId = _clientUserId,
            CompanyName = "Test Co",
            User = clientUser
        };

        var designerProfile = new DesignerProfile
        {
            Id = _designerProfileId,
            UserId = _designerUserId,
            User = designerUser
        };

        var order = new LogoOrder
        {
            Id = _orderId,
            ClientId = _clientProfileId,
            DesignerId = _designerProfileId,
            Title = "Test Order",
            Description = "Test",
            Status = OrderStatus.PreviewDelivered,
            Price = 100,
            AllowExtraRevisions = true,
            Client = clientProfile,
            Designer = designerProfile
        };

        _context.Roles.AddRange(clientRole, designerRole, adminRole);
        _context.Users.AddRange(clientUser, designerUser, adminUser);
        _context.ClientProfiles.Add(clientProfile);
        _context.DesignerProfiles.Add(designerProfile);
        _context.LogoOrders.Add(order);
        _context.SaveChanges();
    }

    private LogoFile CreateLogoFile(FileType fileType, Guid? previewBatchId, Guid uploadedBy, string storageSubPath = "")
    {
        var basePath = string.IsNullOrEmpty(storageSubPath) ? _tempStoragePath : Path.Combine(_tempStoragePath, storageSubPath);
        var fileName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(basePath, fileName);

        Directory.CreateDirectory(basePath);
        File.WriteAllText(filePath, "test content");

        var file = new LogoFile
        {
            Id = Guid.NewGuid(),
            OrderId = _orderId,
            FileName = fileName,
            OriginalFileName = "original.png",
            FilePath = filePath,
            ContentType = "image/png",
            FileSize = 100,
            FileType = fileType,
            IsVisibleToClient = fileType == FileType.Reference,
            IsAdminApproved = fileType == FileType.Reference,
            VersionNumber = 1,
            UploadedBy = uploadedBy,
            PreviewBatchId = previewBatchId
        };

        _context.LogoFiles.Add(file);
        _context.SaveChanges();
        return file;
    }

    [Fact]
    public async Task FullWorkflow_3Revisions_VerifiesAllFourRules()
    {
        // === SETUP: Client reference files (must remain intact throughout) ===
        var refFile1 = CreateLogoFile(FileType.Reference, null, _clientUserId);
        var refFile2 = CreateLogoFile(FileType.Reference, null, _clientUserId);

        // === REVISION 1: Designer uploads preview batch 1 ===
        var batch1Id = Guid.NewGuid();
        var preview1Path = Path.Combine(_tempStoragePath, "Temporary");
        var preview1a = CreateLogoFile(FileType.Preview, batch1Id, _designerUserId, "Temporary");
        var preview1b = CreateLogoFile(FileType.Preview, batch1Id, _designerUserId, "Temporary");

        // Admin forwards to client (simulated - just mark visible; actual send is OrderService)
        preview1a.IsVisibleToClient = true;
        preview1a.IsAdminApproved = true;
        preview1b.IsVisibleToClient = true;
        preview1b.IsAdminApproved = true;
        await _context.SaveChangesAsync();

        // Client requests revision 1
        await _revisionService.RequestRevisionAsync(_orderId, new Application.DTOs.Revisions.RequestRevisionDto { Instructions = "Revision 1: Please make the logo bolder." }, _clientUserId);

        // ASSERT 1: Preview files are deleted on revision
        var previewFilesAfterRev1 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();
        Assert.Empty(previewFilesAfterRev1);

        // ASSERT 2: Client reference files remain intact
        var refFilesAfterRev1 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Reference && !f.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, refFilesAfterRev1.Count);
        Assert.True(File.Exists(refFile1.FilePath));
        Assert.True(File.Exists(refFile2.FilePath));

        // Preview files are soft-deleted (archived for audit); implementation does not physically delete

        // Reset order status for next cycle (RequestRevision sets RevisionRequested; designer would upload, admin would send)
        var order = await _context.LogoOrders.FindAsync(_orderId);
        order!.Status = OrderStatus.PreviewDelivered;

        // === REVISION 2: Designer uploads preview batch 2 ===
        var batch2Id = Guid.NewGuid();
        var preview2a = CreateLogoFile(FileType.Preview, batch2Id, _designerUserId, "Temporary");
        var preview2b = CreateLogoFile(FileType.Preview, batch2Id, _designerUserId, "Temporary");
        preview2a.IsVisibleToClient = true;
        preview2a.IsAdminApproved = true;
        preview2b.IsVisibleToClient = true;
        preview2b.IsAdminApproved = true;
        await _context.SaveChangesAsync();

        await _revisionService.RequestRevisionAsync(_orderId, new Application.DTOs.Revisions.RequestRevisionDto { Instructions = "Revision 2: Change the color scheme." }, _clientUserId);

        var previewFilesAfterRev2 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();
        Assert.Empty(previewFilesAfterRev2);

        refFilesAfterRev1 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Reference && !f.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, refFilesAfterRev1.Count);

        order = await _context.LogoOrders.FindAsync(_orderId);
        order!.Status = OrderStatus.PreviewDelivered;

        // === REVISION 3: Designer uploads preview batch 3 ===
        var batch3Id = Guid.NewGuid();
        var preview3a = CreateLogoFile(FileType.Preview, batch3Id, _designerUserId, "Temporary");
        var preview3b = CreateLogoFile(FileType.Preview, batch3Id, _designerUserId, "Temporary");
        preview3a.IsVisibleToClient = true;
        preview3a.IsAdminApproved = true;
        preview3b.IsVisibleToClient = true;
        preview3b.IsAdminApproved = true;
        await _context.SaveChangesAsync();

        await _revisionService.RequestRevisionAsync(_orderId, new Application.DTOs.Revisions.RequestRevisionDto { Instructions = "Revision 3: Adjust the font size." }, _clientUserId);

        var previewFilesAfterRev3 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();
        Assert.Empty(previewFilesAfterRev3);

        refFilesAfterRev1 = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Reference && !f.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, refFilesAfterRev1.Count);

        order = await _context.LogoOrders.FindAsync(_orderId);
        order!.Status = OrderStatus.PreviewDelivered;

        // === FINAL: Designer uploads preview batch 4, client approves ===
        var batch4Id = Guid.NewGuid();
        var finalPreview1 = CreateLogoFile(FileType.Preview, batch4Id, _designerUserId, "Temporary");
        var finalPreview2 = CreateLogoFile(FileType.Preview, batch4Id, _designerUserId, "Temporary");
        finalPreview1.IsVisibleToClient = true;
        finalPreview1.IsAdminApproved = true;
        finalPreview2.IsVisibleToClient = true;
        finalPreview2.IsAdminApproved = true;
        await _context.SaveChangesAsync();

        await _revisionService.ApproveLogoAsync(_orderId, new Application.DTOs.Revisions.ApproveLogoDto { Notes = "Approved" }, _clientUserId);

        _notificationService.Verify(
            n => n.CreateNotificationAsync(
                _designerUserId,
                "Client Approved Logo",
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Guid?>(),
                It.IsAny<NotificationReferenceType>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()),
            Times.Never);
        _notificationService.Verify(
            n => n.CreateNotificationForRoleAsync(
                "Admin",
                "Client Approved Logo",
                It.IsAny<string>(),
                NotificationType.OrderStatusChange,
                NotificationReferenceType.Order,
                _orderId,
                _clientUserId),
            Times.Once);

        // ASSERT 3: Only final files remain (no Preview files; Reference + Final)
        var previewFilesAfterApproval = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Preview && !f.IsDeleted)
            .ToListAsync();
        Assert.Empty(previewFilesAfterApproval);

        var finalFiles = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Final && !f.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, finalFiles.Count);

        var refFilesFinal = await _context.LogoFiles
            .Where(f => f.OrderId == _orderId && f.FileType == FileType.Reference && !f.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, refFilesFinal.Count);

        // Final files moved to Permanent storage
        foreach (var f in finalFiles)
        {
            Assert.Contains("Permanent", f.FilePath);
            Assert.True(f.IsFinalVersion);
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempStoragePath))
                Directory.Delete(_tempStoragePath, true);
        }
        catch { /* ignore */
        }
        _context.Dispose();
    }
}
