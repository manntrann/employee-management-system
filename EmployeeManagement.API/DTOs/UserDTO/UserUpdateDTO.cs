using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.UserDTO
{
    public class UserUpdateDTO
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(6)]
        public string? Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
