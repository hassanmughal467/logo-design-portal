using BCrypt.Net;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Interfaces;

namespace LogoDesignPortal.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<Domain.Entities.Role> _roleRepository;

    public UserService(
        IRepository<Domain.Entities.User> userRepository,
        IRepository<Domain.Entities.Role> roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequest request)
    {
        // Check if email already exists
        var existingUsers = await _userRepository.FindAsync(u => u.Email == request.Email);
        if (existingUsers.Any())
        {
            throw new InvalidOperationException("Email already exists.");
        }

        // Verify role exists
        var role = await _roleRepository.GetByIdAsync(request.RoleId);
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

        await _userRepository.AddAsync(user);

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
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var role = await _roleRepository.GetByIdAsync(user.RoleId);

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var roles = await _roleRepository.GetAllAsync();
        var roleDict = roles.ToDictionary(r => r.Id);

        return users.Select(user => new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = roleDict.TryGetValue(user.RoleId, out var role) ? role.Name : string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();
    }
}
