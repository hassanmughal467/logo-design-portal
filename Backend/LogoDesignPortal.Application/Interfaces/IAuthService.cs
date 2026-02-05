using LogoDesignPortal.Application.DTOs.Auth;

namespace LogoDesignPortal.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<string> GenerateTokenAsync(Domain.Entities.User user);
}
