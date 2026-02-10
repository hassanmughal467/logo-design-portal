using LogoDesignPortal.Application.DTOs.Users;

namespace LogoDesignPortal.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(Guid id, Guid deletedBy);
    Task<DesignerProfileResponseDto> CreateDesignerProfileAsync(CreateDesignerProfileRequestDto request);
    Task<DesignerProfileResponseDto?> GetDesignerProfileByUserIdAsync(Guid userId);
    Task<List<DesignerProfileResponseDto>> GetAllDesignerProfilesAsync();
}
