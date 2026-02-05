using LogoDesignPortal.Domain.Entities;

namespace LogoDesignPortal.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
}
