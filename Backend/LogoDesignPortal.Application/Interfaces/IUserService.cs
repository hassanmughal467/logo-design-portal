using LogoDesignPortal.Application.DTOs.Users;

namespace LogoDesignPortal.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserRequest request);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<List<UserResponseDto>> GetAllUsersAsync();
}
