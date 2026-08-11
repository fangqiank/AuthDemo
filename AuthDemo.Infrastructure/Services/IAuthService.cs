using AuthDemo.Core.Models;

namespace AuthDemo.Infrastructure.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User?> GetUserByApiKeyAsync(string apiKey);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> ValidateSessionAsync(string sessionId);
    }
}
