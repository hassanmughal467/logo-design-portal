using LogoDesignPortal.Application.DTOs.Auth;

namespace LogoDesignPortal.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
    Task ResetPasswordAsync(Guid targetUserId, ResetPasswordRequestDto request);
    Task ResetSuperAdminPasswordAsync();
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequestDto request);
}
