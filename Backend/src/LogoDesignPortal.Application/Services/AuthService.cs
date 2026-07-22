using BCrypt.Net;
using LogoDesignPortal.Application.BackgroundJobs;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Application.Services;

public class AuthService : IAuthService
{
    private const string ForgotPasswordGenericMessage =
        "If an account exists for that email address, a password reset link will be sent.";

    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly IBackgroundJobScheduler _backgroundJobs;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        INotificationService notificationService,
        IConfiguration configuration,
        IBackgroundJobScheduler backgroundJobs,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _notificationService = notificationService;
        _configuration = configuration;
        _backgroundJobs = backgroundJobs;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var lockoutMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            throw new UnauthorizedAccessException($"Account is locked. Please try again in {lockoutMinutes} minute(s).");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            
            // Lock account after 5 failed attempts for 30 minutes
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Account locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }
            
            await _context.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        var token = await _jwtTokenService.GenerateTokenAsync(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = GetAccessTokenExpiry();
        var refreshTokenHours = int.TryParse(_configuration["Jwt:RefreshTokenExpiryHours"], out var rth) ? rth : 168;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(refreshTokenHours);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiry,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = user.Role?.Name ?? string.Empty
            }
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted);

        if (emailExists)
        {
            throw new InvalidOperationException("User already exist");
        }

        // Get Client role
        var clientRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Client");

        if (clientRole == null)
        {
            throw new InvalidOperationException("Client role not found.");
        }

        // Create user (profile will be completed later) - do NOT set Role nav property to avoid EF tracking issues
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = clientRole.Id,
            IsActive = true,
            SecondaryEmail = request.SecondaryEmail,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create ClientProfile so the client can create orders immediately (CompanyName is required in RegisterRequestDto)
        var companyName = (request.CompanyName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(companyName))
        {
            throw new InvalidOperationException("Company name is required.");
        }
        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = companyName,
            ContactName = $"{user.FirstName} {user.LastName}".Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.ClientProfiles.Add(clientProfile);

        await _context.SaveChangesAsync();

        // Notify Admin and SuperAdmin: New client registered
        try
        {
            var clientName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = companyName;
            var title = "New Client Registered";
            var message = $"New client registered: {clientName}";
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Info, NotificationReferenceType.System, user.Id);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Info, NotificationReferenceType.System, user.Id);
        }
        catch
        {
            // Must not fail registration
        }

        // Reload user with Role for JWT token generation (avoids EF tracking issues with new entities)
        var userWithRole = await _context.Users
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id);

        var token = await _jwtTokenService.GenerateTokenAsync(userWithRole);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = GetAccessTokenExpiry();
        var refreshTokenHours = int.TryParse(_configuration["Jwt:RefreshTokenExpiryHours"], out var rth) ? rth : 168;

        userWithRole.RefreshToken = refreshToken;
        userWithRole.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(refreshTokenHours);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiry,
            User = new UserDto
            {
                Id = userWithRole.Id,
                Email = userWithRole.Email,
                FirstName = userWithRole.FirstName,
                LastName = userWithRole.LastName,
                RoleName = userWithRole.Role?.Name ?? clientRole.Name
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var userId = _jwtTokenService.GetUserIdFromToken(request.Token);

        if (userId == null)
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var newToken = await _jwtTokenService.GenerateTokenAsync(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = GetAccessTokenExpiry();
        var refreshTokenHours = int.TryParse(_configuration["Jwt:RefreshTokenExpiryHours"], out var rth) ? rth : 168;

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(refreshTokenHours);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = accessTokenExpiry,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = user.Role?.Name ?? string.Empty
            }
        };
    }

    private DateTime GetAccessTokenExpiry()
    {
        if (int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes) && minutes > 0)
            return DateTime.UtcNow.AddMinutes(minutes);
        if (int.TryParse(_configuration["Jwt:AccessTokenExpiryHours"], out var hours))
            return DateTime.UtcNow.AddHours(hours);
        return DateTime.UtcNow.AddMinutes(15);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        // Validate that new password and confirm password match
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirmation password do not match.");
        }

        // Basic password strength validation (relaxed)
        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        // Get user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Check if new password is different from current password
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }

        // Update password with higher work factor for better security
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        // Invalidate any existing refresh token so a stolen one doesn't outlive this password change.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(Guid targetUserId, ResetPasswordRequestDto request)
    {
        // Basic validation
        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        // Get target user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        // Invalidate any existing refresh token so a stolen one doesn't outlive this reset.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ResetSuperAdminPasswordAsync()
    {
        var superAdmin = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == "superadmin@logodesign.com" && !u.IsDeleted);

        if (superAdmin == null)
        {
            throw new InvalidOperationException("SuperAdmin user not found.");
        }

        const string defaultPassword = "SuperAdmin@123";
        superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
        superAdmin.IsActive = true;
        superAdmin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var isDevelopment = IsDevelopmentEnvironment();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted && u.IsActive);

        if (user == null)
        {
            _logger.LogInformation("Forgot password requested for unknown or inactive email.");
            return CreateGenericForgotPasswordResponse();
        }

        // Generate reset token (using a secure random token)
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Build reset link
        var frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

        var smtpConfigured = !string.IsNullOrWhiteSpace(_configuration["Email:SmtpServer"])
            && !string.IsNullOrWhiteSpace(_configuration["Email:SmtpUsername"])
            && !string.IsNullOrWhiteSpace(_configuration["Email:SmtpPassword"])
            && !string.IsNullOrWhiteSpace(_configuration["Email:FromEmail"]);

        // Non-blocking: password reset mail is sent by Hangfire worker.
        if (smtpConfigured)
        {
            try
            {
                _backgroundJobs.EnqueuePasswordResetEmail(user.Email, resetLink, $"{user.FirstName} {user.LastName}".Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue password reset email.");
            }
        }
        else if (!isDevelopment)
        {
            _logger.LogError(
                "Password reset email is not configured in {Environment}; reset token was generated but not returned.",
                GetEnvironmentName());
        }

        if (isDevelopment)
        {
            return new ForgotPasswordResponseDto
            {
                Message = "Password reset link generated for local development.",
                ResetToken = token,
                Email = user.Email,
                ResetLink = resetLink
            };
        }

        return CreateGenericForgotPasswordResponse();
    }

    public async Task ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequestDto request)
    {
        // Validate that new password and confirm password match
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirmation password do not match.");
        }

        // Basic password strength validation
        if (request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        // Get user by email and token
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && 
                u.PasswordResetToken == request.Token && 
                !u.IsDeleted && 
                u.IsActive);

        if (user == null)
        {
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        // Check if token is expired
        if (user.PasswordResetTokenExpiryTime == null || 
            user.PasswordResetTokenExpiryTime < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Reset token has expired. Please request a new password reset.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        user.PasswordResetToken = null; // Clear token after use
        user.PasswordResetTokenExpiryTime = null;
        // Invalidate any existing refresh token so a stolen one doesn't outlive this reset.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private ForgotPasswordResponseDto CreateGenericForgotPasswordResponse() =>
        new() { Message = ForgotPasswordGenericMessage };

    private bool IsDevelopmentEnvironment() =>
        string.Equals(GetEnvironmentName(), "Development", StringComparison.OrdinalIgnoreCase);

    private string GetEnvironmentName() =>
        _configuration["ASPNETCORE_ENVIRONMENT"]
        ?? _configuration["DOTNET_ENVIRONMENT"]
        ?? string.Empty;
}
