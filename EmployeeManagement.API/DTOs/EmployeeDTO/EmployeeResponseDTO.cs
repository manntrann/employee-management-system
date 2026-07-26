namespace EmployeeManagement.API.DTOs.EmployeeDTO
{
    public class EmployeeResponseDTO
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Position { get; set; }

        public decimal Salary { get; set; }

        public string? Phone { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
