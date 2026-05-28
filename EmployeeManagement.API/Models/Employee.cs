using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Position { get; set; }

        public decimal Salary { get; set; }

        public string? Phone { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
