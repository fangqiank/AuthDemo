using AuthDemo.Core.Models;

namespace AuthDemo.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        // 模拟数据库
        private static readonly List<User> Users = new()
        {
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                ApiKey = "demo-api-key-12345",
                Roles = ["Admin", "User"]
            },
            new User
            {
                Id = 2,
                Username = "user",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                ApiKey = "demo-api-key-67890",
                Roles = ["User"]
            }
        };

        public Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
                return Task.FromResult<User?>(null);

            user.LastLoginAt = DateTime.UtcNow;
            return Task.FromResult<User?>(user);
        }

        public Task<User?> GetUserByApiKeyAsync(string apiKey) => Task.FromResult<User?>(Users.FirstOrDefault(u => u.ApiKey == apiKey));

        public Task<User?> GetUserByIdAsync(int id) => Task.FromResult<User?>(Users.FirstOrDefault(u => u.Id == id));

        public Task<bool> ValidateSessionAsync(string sessionId) => Task.FromResult(!string.IsNullOrEmpty(sessionId) && sessionId.StartsWith("session_"));
    }
}
