using EmployeeManagement.API.DTOs.DepartmentDTO;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentResponseDTO>> GetAll();

        Task<Department> Create(DepartmentDTO dto);
    }
}
