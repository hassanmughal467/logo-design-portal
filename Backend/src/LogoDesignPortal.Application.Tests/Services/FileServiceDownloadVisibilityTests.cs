using AutoMapper;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// SEC: Clients must not download undelivered designer preview files (IDOR via file GUID).
/// </summary>
public class FileServiceDownloadVisibilityTests : IDisposable
{
    private readonly string _tempStoragePath;
    private readonly ApplicationDbContext _context;
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _hiddenFileId = Guid.NewGuid();

    public FileServiceDownloadVisibilityTests()
    {
        _tempStoragePath = Path.Combine(Path.GetTempPath(), "LDP_DownloadVis_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempStoragePath);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("DownloadVis_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        Seed();
    }

    private void Seed()
    {
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientProfileId = Guid.NewGuid();
        _context.Roles.Add(clientRole);
        _context.Users.Add(new User
        {
            Id = _clientUserId,
            Email = "c@test.com",
            FirstName = "C",
            LastName = "1",
            PasswordHash = "x",
            RoleId = clientRole.Id,
            Role = clientRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientProfileId,
            UserId = _clientUserId,
            CreatedAt = DateTime.UtcNow
        });
        _context.LogoOrders.Add(new LogoOrder
        {
            Id = _orderId,
            ClientId = clientProfileId,
            Status = OrderStatus.InProgress,
            AllowUploads = true,
            CreatedAt = DateTime.UtcNow
        });

        var physicalPath = Path.Combine(_tempStoragePath, "hidden.png");
        File.WriteAllBytes(physicalPath, new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        _context.LogoFiles.Add(new LogoFile
        {
            Id = _hiddenFileId,
            OrderId = _orderId,
            FileName = "hidden.png",
            OriginalFileName = "hidden.png",
            FilePath = physicalPath,
            ContentType = "image/png",
            FileSize = 4,
            FileType = FileType.Preview,
            IsVisibleToClient = false,
            UploadedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task DownloadFileAsync_ClientHiddenPreview_ThrowsForbidden()
    {
        var service = CreateFileService();
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.DownloadFileAsync(_hiddenFileId, _clientUserId, "Client"));
    }

    [Fact]
    public async Task DownloadFileAsync_AdminHiddenPreview_Allowed()
    {
        var service = CreateFileService();
        var (_, name, _) = await service.DownloadFileAsync(_hiddenFileId, Guid.NewGuid(), "Admin");
        Assert.Equal("hidden.png", name);
    }

    private FileService CreateFileService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Path"] = _tempStoragePath }!)
            .Build();
        var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        return new FileService(
            _context,
            config,
            mapper,
            Mock.Of<ILogger<FileService>>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Mock.Of<IDesignerPayoutService>(),
            Mock.Of<IFileUploadScanHook>(),
            Options.Create(new ProductionSafetyOptions()));
    }

    public void Dispose() => Directory.Delete(_tempStoragePath, recursive: true);
}
