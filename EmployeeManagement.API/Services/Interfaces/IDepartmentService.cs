using EmployeeManagement.API.DTOs.DepartmentDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Results;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentResponseDTO>> GetAll();

        Task<DepartmentResponseDTO?> GetById(int id);

        Task<Department> Create(DepartmentDTO dto);

        Task<bool> Update(int id, DepartmentDTO dto);

        Task<DepartmentDeleteResult> Delete(int id);
    }
}
