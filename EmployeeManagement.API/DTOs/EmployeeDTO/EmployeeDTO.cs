using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.EmployeeDTO
{
    public class EmployeeDTO
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}
