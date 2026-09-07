namespace AuthDemoAgain.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string Department { get; set; } = "General";
    }

    public static class MockUserStore
    {
        public static List<User> Users = new()
        {
            new User { Username = "admin", Password = "admin123", Role = "Admin", Department = "IT" },
            new User { Username = "user", Password = "user123", Role = "User", Department = "Sales" }
        };
    }
}
