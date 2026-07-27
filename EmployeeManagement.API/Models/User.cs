namespace EmployeeManagement.API.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? TemporaryPasswordHash { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
