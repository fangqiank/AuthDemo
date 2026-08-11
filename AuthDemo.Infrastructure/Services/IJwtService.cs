using AuthDemo.Core.Models;

namespace AuthDemo.Infrastructure.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
        bool ValidateToken(string token);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    }
}
