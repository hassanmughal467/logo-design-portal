using BCrypt.Net;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Authentication;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _configuration = configuration;
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

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
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

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = clientRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create client profile
        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = request.CompanyName,
            PhoneNumber = request.Phone ?? string.Empty,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            PostalCode = request.ZipCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClientProfiles.Add(clientProfile);
        await _context.SaveChangesAsync();

        // Generate tokens
        var token = await _jwtTokenService.GenerateTokenAsync(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = clientRole.Name
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

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
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

        // Reset to default password
        superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123");
        superAdmin.IsActive = true;
        superAdmin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted && u.IsActive);

        // Check if user exists
        if (user == null)
        {
            throw new InvalidOperationException("No user exists with this email address.");
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

        // Send email with reset link
        var emailSent = await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            resetLink,
            $"{user.FirstName} {user.LastName}");

        // For development: If email fails, still return the link in response
        var isDevelopment = _configuration["ASPNETCORE_ENVIRONMENT"] == "Development" || 
                           string.IsNullOrEmpty(_configuration["Email:SmtpServer"]);

        return new ForgotPasswordResponseDto
        {
            Message = emailSent 
                ? "Password reset link has been sent to your email address." 
                : "Password reset link generated. Please check the link below (email sending is not configured).",
            // Only include these in development or if email failed
            ResetToken = isDevelopment || !emailSent ? token : null,
            Email = isDevelopment || !emailSent ? user.Email : null,
            ResetLink = isDevelopment || !emailSent ? resetLink : null
        };
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
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
