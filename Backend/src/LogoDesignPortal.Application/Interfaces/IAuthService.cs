using LogoDesignPortal.Application.DTOs.Auth;

namespace LogoDesignPortal.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
