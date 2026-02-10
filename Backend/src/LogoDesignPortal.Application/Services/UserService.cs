using BCrypt.Net;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;

    public UserService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException("User already exist");
        }

        // Verify role exists
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId);

        if (role == null)
        {
            throw new InvalidOperationException("Invalid role specified.");
        }

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = role.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            return null;
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        return users.Select(user => new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Check if email is being changed and if new email already exists
        if (user.Email != request.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != id);

            if (emailExists)
            {
                throw new InvalidOperationException("User already exist");
            }
        }

        // Find role by name
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.Role);

        if (role == null)
        {
            throw new InvalidOperationException($"Invalid role specified: {request.Role}");
        }

        // Update user properties
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.RoleId = role.Id;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Use the role we already loaded
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = role.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteUserAsync(Guid id, Guid deletedBy)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Prevent deleting SuperAdmin users
        if (user.Role?.Name == "SuperAdmin")
        {
            throw new InvalidOperationException("Cannot delete SuperAdmin users.");
        }

        // Prevent users from deleting themselves
        if (user.Id == deletedBy)
        {
            throw new InvalidOperationException("Cannot delete your own account.");
        }

        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = deletedBy;
        user.IsActive = false; // Also deactivate the user

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DesignerProfileResponseDto> CreateDesignerProfileAsync(CreateDesignerProfileRequestDto request)
    {
        // Check if user exists and has Designer role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role?.Name != "Designer")
        {
            throw new InvalidOperationException("User is not a Designer.");
        }

        // Check if profile already exists
        var existingProfile = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == request.UserId && !d.IsDeleted);

        if (existingProfile != null)
        {
            throw new InvalidOperationException("Designer profile already exists for this user.");
        }

        var designerProfile = new Domain.Entities.DesignerProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Specialization = request.Specialization,
            Bio = request.Bio,
            HourlyRate = request.HourlyRate,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        _context.DesignerProfiles.Add(designerProfile);
        await _context.SaveChangesAsync();

        return new DesignerProfileResponseDto
        {
            Id = designerProfile.Id,
            UserId = user.Id,
            UserEmail = user.Email,
            UserFirstName = user.FirstName,
            UserLastName = user.LastName,
            Specialization = designerProfile.Specialization,
            Bio = designerProfile.Bio,
            HourlyRate = designerProfile.HourlyRate,
            IsAvailable = designerProfile.IsAvailable,
            CreatedAt = designerProfile.CreatedAt
        };
    }

    public async Task<DesignerProfileResponseDto?> GetDesignerProfileByUserIdAsync(Guid userId)
    {
        var profile = await _context.DesignerProfiles
            .Include(d => d.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

        if (profile == null)
        {
            return null;
        }

        return new DesignerProfileResponseDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserEmail = profile.User.Email,
            UserFirstName = profile.User.FirstName,
            UserLastName = profile.User.LastName,
            Specialization = profile.Specialization,
            Bio = profile.Bio,
            HourlyRate = profile.HourlyRate,
            IsAvailable = profile.IsAvailable,
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<List<DesignerProfileResponseDto>> GetAllDesignerProfilesAsync()
    {
        var profiles = await _context.DesignerProfiles
            .Include(d => d.User)
            .ThenInclude(u => u.Role)
            .Where(d => !d.IsDeleted && d.User.Role.Name == "Designer")
            .ToListAsync();

        return profiles.Select(profile => new DesignerProfileResponseDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserEmail = profile.User.Email,
            UserFirstName = profile.User.FirstName,
            UserLastName = profile.User.LastName,
            Specialization = profile.Specialization,
            Bio = profile.Bio,
            HourlyRate = profile.HourlyRate,
            IsAvailable = profile.IsAvailable,
            CreatedAt = profile.CreatedAt
        }).ToList();
    }
}
