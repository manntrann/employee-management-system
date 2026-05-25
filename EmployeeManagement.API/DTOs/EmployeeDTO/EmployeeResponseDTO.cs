namespace EmployeeManagement.API.DTOs.EmployeeDTO
{
    public class EmployeeResponseDTO
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string? Email { get; set; }

        public string? Position { get; set; }

        public decimal Salary { get; set; }

        public string? Phone { get; set; }

        public string DepartmentName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
