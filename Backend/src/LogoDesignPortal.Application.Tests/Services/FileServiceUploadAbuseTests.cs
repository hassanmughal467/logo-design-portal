using AutoMapper;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Tests.TestHelpers;
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
/// Upload abuse: extension, magic bytes, path traversal filename, duplicate window.
/// </summary>
public class FileServiceUploadAbuseTests : IDisposable
{
    private readonly string _tempStoragePath;
    private readonly ApplicationDbContext _context;
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public FileServiceUploadAbuseTests()
    {
        _tempStoragePath = Path.Combine(Path.GetTempPath(), "LDP_UploadAbuse_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempStoragePath);
        Directory.CreateDirectory(Path.Combine(_tempStoragePath, "Temporary"));

        _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("UploadAbuse_" + Guid.NewGuid())
            .Options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        _context.Roles.Add(clientRole);
        _context.Users.Add(new User
        {
            Id = _clientUserId,
            Email = "c@abuse.test",
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
            Id = _clientProfileId,
            UserId = _clientUserId,
            CreatedAt = DateTime.UtcNow
        });
        _context.LogoOrders.Add(new LogoOrder
        {
            Id = _orderId,
            ClientId = _clientProfileId,
            Status = OrderStatus.InProgress,
            AllowUploads = true,
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task UploadFile_ExeExtension_ThrowsInvalidOperation()
    {
        var service = CreateService();
        var file = FormFileTestHelper.Create("virus.exe", "application/octet-stream",
            [0x4D, 0x5A, 0x90, 0x00]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadFileAsync(_orderId, file, _clientUserId, "Reference"));
    }

    [Fact]
    public async Task UploadFile_PngExtensionPdfContent_ThrowsInvalidOperation()
    {
        var service = CreateService();
        var file = FormFileTestHelper.Create("fake.png", "image/png",
            [0x25, 0x50, 0x44, 0x46]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadFileAsync(_orderId, file, _clientUserId, "Reference"));
    }

    [Fact]
    public async Task UploadFile_PathTraversalOriginalName_StoresSanitizedName()
    {
        var service = CreateService();
        var file = FormFileTestHelper.Create("../../../etc/passwd.png", "image/png");

        var result = await service.UploadFileAsync(_orderId, file, _clientUserId, "Reference");
        Assert.DoesNotContain("..", result.OriginalFileName);
        Assert.EndsWith(".png", result.OriginalFileName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadFile_DuplicateWithinOneHour_ThrowsInvalidOperation()
    {
        var service = CreateService();
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file1 = FormFileTestHelper.Create("dup.png", "image/png", bytes);
        await service.UploadFileAsync(_orderId, file1, _clientUserId, "Reference");

        var file2 = FormFileTestHelper.Create("dup.png", "image/png", bytes);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadFileAsync(_orderId, file2, _clientUserId, "Reference"));
    }

    [Fact]
    public async Task UploadFile_MismatchedContentTypeForPng_ThrowsWhenDeclared()
    {
        var service = CreateService();
        var file = FormFileTestHelper.Create("ok.png", "application/pdf",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadFileAsync(_orderId, file, _clientUserId, "Reference"));
    }

    private FileService CreateService()
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
