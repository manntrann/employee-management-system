using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.EmployeeDTO
{
    public class EmployeeProfileUpdateDTO
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Position { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [MinLength(6)]
        public string? Password { get; set; }
    }
}
