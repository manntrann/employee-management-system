using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.LoginDTO
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
