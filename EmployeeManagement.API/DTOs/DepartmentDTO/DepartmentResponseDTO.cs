using EmployeeManagement.API.DTOs.EmployeeDTO;

namespace EmployeeManagement.API.DTOs.DepartmentDTO
{
    public class DepartmentResponseDTO
    {
        public int Id { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public List<EmployeeResponseDTO> Employee { get; set; } = new();
    }
}
