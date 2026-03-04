using LogoDesignPortal.Application.DTOs.Users;

namespace LogoDesignPortal.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<UserResponseDto> UpdateUserProfileAsync(Guid id, UpdateClientProfileDto request);
    Task<bool> DeleteUserAsync(Guid id, Guid deletedBy);
    Task<bool> SoftDeactivateUserAsync(Guid id, Guid deactivatedBy);
    Task<bool> ReactivateUserAsync(Guid id);
    Task<bool> HardDeleteUserAsync(Guid id, Guid deletedBy);
    Task<DesignerProfileResponseDto> CreateDesignerProfileAsync(CreateDesignerProfileRequestDto request);
    Task<DesignerProfileResponseDto?> GetDesignerProfileByUserIdAsync(Guid userId);
    Task<List<DesignerProfileResponseDto>> GetAllDesignerProfilesAsync();
    Task<ClientDetailDto?> GetClientDetailAsync(Guid clientId);
    Task<DesignerDetailDto?> GetDesignerDetailAsync(Guid designerId);
}
