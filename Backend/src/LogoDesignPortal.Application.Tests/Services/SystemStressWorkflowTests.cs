using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LogoDesignPortal.Application;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure;
using LogoDesignPortal.Infrastructure.Persistence;
using LogoDesignPortal.Application.Tests.TestHelpers;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Load and concurrency stress test simulating 100 concurrent orders
/// with full workflow: create, assign, preview uploads, revisions, and final approval.
/// </summary>
public class SystemStressWorkflowTests : IAsyncDisposable
{
    private const int OrderCount = 100;
    private readonly string _storagePath;
    private readonly string _dbName;
    private readonly IServiceProvider _serviceProvider;
    private readonly Guid _adminUserId;
    private readonly Guid _designerUserId;
    private readonly Guid[] _clientUserIds;
    private readonly Guid _designerProfileId;

    public SystemStressWorkflowTests()
    {
        _storagePath = Path.Combine(Path.GetTempPath(), "LogoDesignPortal_Stress_" + Guid.NewGuid().ToString("N")[..8]);
        _dbName = "StressTest_" + Guid.NewGuid().ToString("N");

        Directory.CreateDirectory(_storagePath);
        Directory.CreateDirectory(Path.Combine(_storagePath, "Temporary"));
        Directory.CreateDirectory(Path.Combine(_storagePath, "Permanent"));

        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:Path"] = _storagePath
            }!)
            .Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddApplication();
        services.AddScoped<INotificationService>(_ => Mock.Of<INotificationService>());
        services.AddScoped<IRealtimeEntityUpdateSender>(_ => Mock.Of<IRealtimeEntityUpdateSender>());
        services.AddScoped<IPermissionService>(sp =>
        {
            var mock = new Mock<IPermissionService>();
            mock.Setup(x => x.UserHasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);
            return mock.Object;
        });

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedDatabase(context, out _adminUserId, out _designerUserId, out _clientUserIds, out _designerProfileId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_storagePath))
                Directory.Delete(_storagePath, true);
        }
        catch { /* ignore */ }
        await Task.CompletedTask;
    }

    private static void SeedDatabase(
        ApplicationDbContext context,
        out Guid adminUserId,
        out Guid designerUserId,
        out Guid[] clientUserIds,
        out Guid designerProfileId)
    {
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var superAdminRole = new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" };

        adminUserId = Guid.NewGuid();
        designerUserId = Guid.NewGuid();
        clientUserIds = new Guid[OrderCount];
        for (int i = 0; i < OrderCount; i++)
            clientUserIds[i] = Guid.NewGuid();

        var adminUser = new User
        {
            Id = adminUserId,
            Email = "admin@stress.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = adminRole.Id,
            Role = adminRole
        };

        var designerUser = new User
        {
            Id = designerUserId,
            Email = "designer@stress.com",
            FirstName = "Designer",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = designerRole.Id,
            Role = designerRole
        };

        var designerProfile = new DesignerProfile
        {
            Id = Guid.NewGuid(),
            UserId = designerUserId,
            User = designerUser
        };
        designerProfileId = designerProfile.Id;

        context.Roles.AddRange(clientRole, designerRole, adminRole, superAdminRole);
        context.Users.Add(adminUser);
        context.Users.Add(designerUser);

        for (int i = 0; i < OrderCount; i++)
        {
            var user = new User
            {
                Id = clientUserIds[i],
                Email = $"client{i}@stress.com",
                FirstName = "Client",
                LastName = $"{i}",
                PasswordHash = "hash",
                RoleId = clientRole.Id,
                Role = clientRole
            };
            context.Users.Add(user);
            context.ClientProfiles.Add(new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserIds[i],
                CompanyName = $"Company {i}",
                User = user
            });
        }

        context.DesignerProfiles.Add(designerProfile);
        context.SaveChanges();
    }

    [Fact]
    public async Task Simulate_100_Concurrent_Orders_With_Revisions()
    {
        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, OrderCount).Select(async orderIndex =>
        {
            var orderStopwatch = Stopwatch.StartNew();
            try
            {
                var (revisions, previewUploads, finalApprovals) = await ExecuteOrderWorkflowAsync(orderIndex);
                return (OrderIndex: orderIndex, Success: true, Error: (string?)null, Elapsed: orderStopwatch.Elapsed, Revisions: revisions, PreviewUploads: previewUploads, FinalApprovals: finalApprovals);
            }
            catch (Exception ex)
            {
                return (OrderIndex: orderIndex, Success: false, Error: ex.ToString(), Elapsed: orderStopwatch.Elapsed, Revisions: 0, PreviewUploads: 0, FinalApprovals: 0);
            }
        });

        var taskResults = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var failures = taskResults.Where(r => !r.Success).ToList();
        var successes = taskResults.Where(r => r.Success).ToList();

        var totalRevisions = successes.Sum(r => r.Revisions);
        var totalPreviewUploads = successes.Sum(r => r.PreviewUploads);
        var totalFinalApprovals = successes.Sum(r => r.FinalApprovals);

        if (failures.Any())
        {
            var sb = new StringBuilder();
            sb.AppendLine($"FAILED: {failures.Count} orders had errors.");
            foreach (var f in failures.Take(5))
                sb.AppendLine($"  Order {f.OrderIndex}: {f.Error?.Substring(0, Math.Min(200, f.Error?.Length ?? 0))}...");
            Assert.Fail(sb.ToString());
        }

        Assert.Equal(OrderCount, successes.Count);

        var avgOrderTime = TimeSpan.FromMilliseconds(successes.Average(r => r.Elapsed.TotalMilliseconds));

        var summary = $"""
            === STRESS TEST SUMMARY ===
            Total orders processed:     {successes.Count}
            Total revisions executed:   {totalRevisions}
            Total preview uploads:      {totalPreviewUploads}
            Total final approvals:      {totalFinalApprovals}
            Total execution time:      {stopwatch.ElapsedMilliseconds} ms
            Avg order processing time:  {avgOrderTime.TotalMilliseconds:F0} ms
            """;

        // Output for CI/log
        Console.WriteLine(summary);
    }

    private async Task<(int Revisions, int PreviewUploads, int FinalApprovals)> ExecuteOrderWorkflowAsync(int orderIndex)
    {
        var clientUserId = _clientUserIds[orderIndex];

        using var scope = _serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var revisionService = scope.ServiceProvider.GetRequiredService<IRevisionService>();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // 1. Client creates order with 2 reference files
        var createRequest = new CreateOrderRequestDto
        {
            Title = $"Stress Order {orderIndex}",
            Description = "Stress test order description for load testing",
            Price = 100m,
            Priority = OrderPriority.Medium
        };
        var refFiles = new[]
        {
            FormFileTestHelper.Create(fileName: $"ref1_{orderIndex}.png"),
            FormFileTestHelper.Create(fileName: $"ref2_{orderIndex}.png")
        };
        var order = await orderService.CreateOrderWithFilesAsync(createRequest, refFiles, clientUserId);
        Assert.NotNull(order);
        var orderId = order.Id;

        // 2. Admin assigns designer (designerId = User.Id)
        order = await orderService.AssignOrderToDesignerAsync(orderId, _designerUserId, _adminUserId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.InProgress.ToString(), order.Status);

        // 3. Designer uploads preview batch (3 files)
        var previewBatch1 = new[]
        {
            FormFileTestHelper.Create(fileName: $"prev1_{orderIndex}.png"),
            FormFileTestHelper.Create(fileName: $"prev2_{orderIndex}.png"),
            FormFileTestHelper.Create(fileName: $"prev3_{orderIndex}.png")
        };
        var uploadResult1 = await fileService.UploadMultipleFilesAsync(orderId, previewBatch1, _designerUserId, "Preview");
        Assert.Equal(3, uploadResult1.Count);
        var previewUploads = 3;

        // 4. Admin forwards preview batch to client
        var fileIds1 = uploadResult1.Select(f => f.Id).ToList();
        order = await orderService.SendFilesToClientAsync(orderId, fileIds1, _adminUserId);
        Assert.Equal(OrderStatus.PreviewDelivered.ToString(), order.Status);

        // 5. Client requests revision (1st revision)
        var revisionRequest = new RequestRevisionDto { Instructions = $"Revision 1 for order {orderIndex}: please adjust colors." };
        await revisionService.RequestRevisionAsync(orderId, revisionRequest, clientUserId);
        var revisions = 1;

        // Verify: preview files deleted, reference files remain
        var filesAfterRev1 = await context.LogoFiles.Where(f => f.OrderId == orderId && !f.IsDeleted).ToListAsync();
        var previewAfterRev1 = filesAfterRev1.Where(f => f.FileType == FileType.Preview).ToList();
        var refAfterRev1 = filesAfterRev1.Where(f => f.FileType == FileType.Reference).ToList();
        Assert.Empty(previewAfterRev1);
        Assert.Equal(2, refAfterRev1.Count);
        foreach (var f in refAfterRev1)
            Assert.True(File.Exists(f.FilePath), $"Reference file missing: {f.FilePath}");

        // 6. Designer uploads new preview batch (3 files)
        var previewBatch2 = new[]
        {
            FormFileTestHelper.Create(fileName: $"prev4_{orderIndex}.png"),
            FormFileTestHelper.Create(fileName: $"prev5_{orderIndex}.png"),
            FormFileTestHelper.Create(fileName: $"prev6_{orderIndex}.png")
        };
        var uploadResult2 = await fileService.UploadMultipleFilesAsync(orderId, previewBatch2, _designerUserId, "Preview");
        Assert.Equal(3, uploadResult2.Count);
        previewUploads += 3;

        // 7. Admin forwards preview batch
        var fileIds2 = uploadResult2.Select(f => f.Id).ToList();
        order = await orderService.SendFilesToClientAsync(orderId, fileIds2, _adminUserId);
        Assert.Equal(OrderStatus.PreviewDelivered.ToString(), order.Status);

        // 8. Client approves final design
        var approveDto = new ApproveLogoDto { Notes = $"Approved order {orderIndex}" };
        order = await revisionService.ApproveLogoAsync(orderId, approveDto, clientUserId);
        var finalApprovals = 1;
        Assert.Equal(OrderStatus.Completed.ToString(), order.Status);

        // Final validation: reference files exist, final files exist, no preview files
        var finalFiles = await context.LogoFiles.Where(f => f.OrderId == orderId && !f.IsDeleted).ToListAsync();
        var refFilesFinal = finalFiles.Where(f => f.FileType == FileType.Reference).ToList();
        var finalFilesList = finalFiles.Where(f => f.FileType == FileType.Final).ToList();
        var previewRemaining = finalFiles.Where(f => f.FileType == FileType.Preview).ToList();

        Assert.Empty(previewRemaining);
        Assert.Equal(2, refFilesFinal.Count);
        Assert.Equal(3, finalFilesList.Count);

        foreach (var f in refFilesFinal)
            Assert.True(File.Exists(f.FilePath), $"Reference file missing after approval: {f.FilePath}");
        foreach (var f in finalFilesList)
        {
            Assert.True(File.Exists(f.FilePath), $"Final file missing: {f.FilePath}");
            Assert.Contains("Permanent", f.FilePath);
        }

        return (revisions, previewUploads, finalApprovals);
    }
}
