using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.DepartmentDTO
{
    public class DepartmentDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
