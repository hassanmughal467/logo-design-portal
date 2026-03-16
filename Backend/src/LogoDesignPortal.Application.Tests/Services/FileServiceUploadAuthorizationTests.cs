using System.IO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Tests.TestHelpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// TEST-002: Verifies FileService upload authorization - Client/Designer can only upload to orders they have access to.
/// </summary>
public class FileServiceUploadAuthorizationTests : IDisposable
{
    private readonly string _tempStoragePath;
    private readonly ApplicationDbContext _context;

    public FileServiceUploadAuthorizationTests()
    {
        _tempStoragePath = Path.Combine(Path.GetTempPath(), "LogoDesignPortal_FileAuth_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempStoragePath);
        Directory.CreateDirectory(Path.Combine(_tempStoragePath, "Temporary"));
        Directory.CreateDirectory(Path.Combine(_tempStoragePath, "Permanent"));

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FileAuth_" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        var clientAUserId = Guid.NewGuid();
        var clientBUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();

        var clientA = new User { Id = clientAUserId, Email = "clientA@test.com", FirstName = "A", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientB = new User { Id = clientBUserId, Email = "clientB@test.com", FirstName = "B", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var designer = new User { Id = designerUserId, Email = "designer@test.com", FirstName = "D", LastName = "Designer", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole };

        var clientAProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientAUserId, CompanyName = "Client A", User = clientA };
        var clientBProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientBUserId, CompanyName = "Client B", User = clientB };
        var designerProfile = new DesignerProfile { Id = Guid.NewGuid(), UserId = designerUserId, User = designer };

        var orderA = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientAProfile.Id,
            DesignerId = designerProfile.Id,
            Title = "Order A",
            Description = "Test",
            Status = OrderStatus.InProgress,
            Price = 100,
            AllowUploads = true,
            Client = clientAProfile,
            Designer = designerProfile
        };

        var orderB = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientBProfile.Id,
            DesignerId = null,
            Title = "Order B",
            Description = "Test",
            Status = OrderStatus.InProgress,
            Price = 100,
            AllowUploads = true,
            Client = clientBProfile,
            Designer = null
        };

        _context.Roles.AddRange(clientRole, designerRole, adminRole);
        _context.Users.AddRange(clientA, clientB, designer);
        _context.ClientProfiles.AddRange(clientAProfile, clientBProfile);
        _context.DesignerProfiles.Add(designerProfile);
        _context.LogoOrders.AddRange(orderA, orderB);
        _context.SaveChanges();
    }

    [Fact]
    public async Task UploadFile_ClientUploadsToAnotherClientsOrder_ThrowsForbiddenAccessException()
    {
        var orderB = _context.LogoOrders.Include(o => o.Client).First(o => o.Client!.CompanyName == "Client B");
        var clientA = _context.ClientProfiles.First(c => c.CompanyName == "Client A");

        var fileService = CreateFileService();
        var file = FormFileTestHelper.Create("ref.png");

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => fileService.UploadFileAsync(orderB.Id, file, clientA.UserId, "Reference"));

        Assert.Contains("don't have access", ex.Message);
    }

    [Fact]
    public async Task UploadFile_DesignerUploadsToOrderNotAssignedToThem_ThrowsForbiddenAccessException()
    {
        var orderB = _context.LogoOrders.Include(o => o.Client).First(o => o.Client!.CompanyName == "Client B");
        var designer = _context.DesignerProfiles.First();

        var fileService = CreateFileService();
        var file = FormFileTestHelper.Create("preview.png");

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => fileService.UploadFileAsync(orderB.Id, file, designer.UserId, "Preview"));

        Assert.Contains("don't have access", ex.Message);
    }

    [Fact]
    public async Task UploadFile_ClientUploadsToOwnOrder_Succeeds()
    {
        var orderA = _context.LogoOrders.Include(o => o.Client).First(o => o.Client!.CompanyName == "Client A");
        var clientA = _context.ClientProfiles.First(c => c.CompanyName == "Client A");

        var fileService = CreateFileService();
        var file = FormFileTestHelper.Create("ref.png");

        var result = await fileService.UploadFileAsync(orderA.Id, file, clientA.UserId, "Reference");

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task UploadFile_DesignerUploadsToAssignedOrder_Succeeds()
    {
        var orderA = _context.LogoOrders.Include(o => o.Client).First(o => o.Client!.CompanyName == "Client A");
        var designer = _context.DesignerProfiles.First();

        var fileService = CreateFileService();
        var file = FormFileTestHelper.Create("preview.png");

        // Designer uploading first Preview batch requires design category, type, and proposed price when order has no ProposedPrice
        var result = await fileService.UploadFileAsync(orderA.Id, file, designer.UserId, "Preview",
            designCategory: (int)DesignCategory.EmbroideryDigitizing,
            designType: (int)DesignType.LeftChest,
            proposedPrice: 100m);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    private FileService CreateFileService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Path"] = _tempStoragePath }!)
            .Build();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<LogoDesignPortal.Application.Mappings.MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        return new FileService(
            _context,
            config,
            mapper,
            Mock.Of<ILogger<FileService>>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Mock.Of<IDesignerPayoutService>(),
            Options.Create(new ProductionSafetyOptions()));
    }

    public void Dispose() => Directory.Delete(_tempStoragePath, recursive: true);
}
