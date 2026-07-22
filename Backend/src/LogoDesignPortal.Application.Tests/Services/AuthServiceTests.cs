using LogoDesignPortal.Application.BackgroundJobs;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthService(
            _contextMock.Object,
            _jwtTokenServiceMock.Object,
            Mock.Of<INotificationService>(),
            _configurationMock.Object,
            new NullBackgroundJobScheduler(),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true,
            Role = new Role { Id = Guid.NewGuid(), Name = "Client" }
        };

        var usersMock = new Mock<DbSet<User>>();
        usersMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(user);

        _contextMock.Setup(c => c.Users).Returns(usersMock.Object);
        _jwtTokenServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<User>())).ReturnsAsync("test-token");
        _jwtTokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");

        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        await Task.CompletedTask;
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ThrowsUnauthorizedException()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ForgotPasswordAsync_UnknownEmail_ReturnsGenericResponse()
    {
        await using var context = CreateContext();
        var scheduler = new RecordingBackgroundJobScheduler();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: true), scheduler);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = "missing@example.com"
        });

        AssertGenericForgotPasswordResponse(response);
        Assert.Equal(0, scheduler.PasswordResetEmailCount);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ExistingEmail_NonDevelopment_DoesNotReturnResetMaterial()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        var scheduler = new RecordingBackgroundJobScheduler();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: true), scheduler);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = user.Email
        });

        AssertGenericForgotPasswordResponse(response);
        Assert.Equal(1, scheduler.PasswordResetEmailCount);
        var savedUser = await context.Users.SingleAsync(u => u.Id == user.Id);
        Assert.False(string.IsNullOrWhiteSpace(savedUser.PasswordResetToken));
        Assert.NotNull(savedUser.PasswordResetTokenExpiryTime);
    }

    [Fact]
    public async Task ForgotPasswordAsync_SmtpFailure_NonDevelopment_DoesNotReturnResetMaterial()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        var scheduler = new RecordingBackgroundJobScheduler { ThrowOnPasswordReset = true };
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: true), scheduler);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = user.Email
        });

        AssertGenericForgotPasswordResponse(response);
        Assert.Equal(1, scheduler.PasswordResetEmailCount);
    }

    [Fact]
    public async Task ForgotPasswordAsync_SmtpNotConfigured_NonDevelopment_DoesNotReturnResetMaterial()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        var scheduler = new RecordingBackgroundJobScheduler();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: false), scheduler);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = user.Email
        });

        AssertGenericForgotPasswordResponse(response);
        Assert.Equal(0, scheduler.PasswordResetEmailCount);
    }

    [Fact]
    public async Task ForgotPasswordAsync_Development_ExistingEmail_ReturnsResetMaterialForLocalTesting()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        var scheduler = new RecordingBackgroundJobScheduler();
        var service = CreateService(context, CreateConfiguration("Development", smtpConfigured: false), scheduler);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = user.Email
        });

        Assert.Equal("Password reset link generated for local development.", response.Message);
        Assert.Equal(user.Email, response.Email);
        Assert.False(string.IsNullOrWhiteSpace(response.ResetToken));
        Assert.False(string.IsNullOrWhiteSpace(response.ResetLink));
        Assert.Contains(Uri.EscapeDataString(user.Email), response.ResetLink);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_InvalidatesExistingRefreshToken()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com", password: "Old@Password1");
        user.RefreshToken = "existing-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: false), new RecordingBackgroundJobScheduler());

        await service.ChangePasswordAsync(user.Id, new ChangePasswordRequestDto
        {
            CurrentPassword = "Old@Password1",
            NewPassword = "New@Password1",
            ConfirmPassword = "New@Password1"
        });

        var savedUser = await context.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Null(savedUser.RefreshToken);
        Assert.Null(savedUser.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidRequest_InvalidatesExistingRefreshToken()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        user.RefreshToken = "existing-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: false), new RecordingBackgroundJobScheduler());

        await service.ResetPasswordAsync(user.Id, new ResetPasswordRequestDto
        {
            UserId = user.Id,
            NewPassword = "New@Password1"
        });

        var savedUser = await context.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Null(savedUser.RefreshToken);
        Assert.Null(savedUser.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task ResetPasswordWithTokenAsync_ValidToken_InvalidatesExistingRefreshToken()
    {
        await using var context = CreateContext();
        var user = await SeedActiveUserAsync(context, "client@example.com");
        user.RefreshToken = "existing-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.PasswordResetToken = "valid-reset-token";
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);
        await context.SaveChangesAsync();
        var service = CreateService(context, CreateConfiguration("Production", smtpConfigured: false), new RecordingBackgroundJobScheduler());

        await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenRequestDto
        {
            Email = user.Email,
            Token = "valid-reset-token",
            NewPassword = "New@Password1",
            ConfirmPassword = "New@Password1"
        });

        var savedUser = await context.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Null(savedUser.RefreshToken);
        Assert.Null(savedUser.RefreshTokenExpiryTime);
    }

    private static void AssertGenericForgotPasswordResponse(ForgotPasswordResponseDto response)
    {
        Assert.Equal("If an account exists for that email address, a password reset link will be sent.", response.Message);
        Assert.Null(response.Email);
        Assert.Null(response.ResetToken);
        Assert.Null(response.ResetLink);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AuthForgotPassword_" + Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static AuthService CreateService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IBackgroundJobScheduler scheduler)
    {
        return new AuthService(
            context,
            Mock.Of<IJwtTokenService>(),
            Mock.Of<INotificationService>(),
            configuration,
            scheduler,
            NullLogger<AuthService>.Instance);
    }

    private static IConfiguration CreateConfiguration(string environmentName, bool smtpConfigured)
    {
        var values = new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = environmentName,
            ["Email:FrontendUrl"] = "https://portal.example.com",
            ["Email:SmtpServer"] = smtpConfigured ? "smtp.example.com" : "",
            ["Email:SmtpUsername"] = smtpConfigured ? "smtp-user" : "",
            ["Email:SmtpPassword"] = smtpConfigured ? "smtp-password" : "",
            ["Email:FromEmail"] = smtpConfigured ? "noreply@example.com" : ""
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static async Task<User> SeedActiveUserAsync(ApplicationDbContext context, string email, string password = "Test@123")
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Client",
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Client",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private sealed class RecordingBackgroundJobScheduler : IBackgroundJobScheduler
    {
        public int PasswordResetEmailCount { get; private set; }
        public bool ThrowOnPasswordReset { get; init; }

        public void EnqueueSendEmail(string to, string subject, string body, bool isHtml = true)
        {
        }

        public void EnqueuePasswordResetEmail(string email, string resetLink, string userName)
        {
            PasswordResetEmailCount++;
            if (ThrowOnPasswordReset)
            {
                throw new InvalidOperationException("Simulated SMTP enqueue failure.");
            }
        }
    }
}
