using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs
{
    public class EmployeeDTO
    {
        [Required]
        public string FullName { get; set; }

        public string? Email { get; set; }

        public string? Position { get; set; }

        public decimal Salary { get; set; } 

        public string? Phone { get; set; }   

        public int DepartmentId { get; set; }
    }
}
