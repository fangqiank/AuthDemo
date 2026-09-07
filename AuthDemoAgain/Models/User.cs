using System.Security.Cryptography;
using System.Text;

namespace AuthDemoAgain.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;  // PBKDF2 哈希，明文只在 README
        public string Role { get; set; } = "User";
        public string Department { get; set; } = "General";
    }

    public static class MockUserStore
    {
        private const int Iterations = 100_000;

        // ponytail: 每用户盐写死为用户名派生；真实系统应随机盐随用户记录入库
        private static byte[] Salt(string username) => Encoding.UTF8.GetBytes($"AuthDemoAgain:{username}");

        public static string Hash(string username, string password) =>
            Convert.ToHexString(Rfc2898DeriveBytes.Pbkdf2(password, Salt(username), Iterations, HashAlgorithmName.SHA256, 32));

        // 恒定时间比较，防时序侧信道
        public static bool Verify(User user, string password)
        {
            var expected = Convert.FromHexString(user.PasswordHash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, Salt(user.Username), Iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        public static List<User> Users = new()
        {
            new User { Username = "admin", PasswordHash = "5B2235045D1C0986D2A6EFD53E923DB59DAE356B6DABF60D9332595DB066F1F2", Role = "Admin", Department = "IT" },
            new User { Username = "user", PasswordHash = "E3BCF2FC1BA043C7C81C97C4D80A423AE04E468E7541FCFE65459075275A8946", Role = "User", Department = "Sales" }
        };
    }
}
