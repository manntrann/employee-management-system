using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<object> GetAll(int page, int pageSize, string? search);

        Task<EmployeeResponseDTO?> GetById(int id);

        Task<EmployeeResponseDTO?> GetByUserId(int userId);

        Task<Employee?> Create(EmployeeDTO dto);

        Task<bool> Update(int id, EmployeeDTO dto);

        Task<EmployeeResponseDTO?> UpdateProfile(int userId, EmployeeProfileUpdateDTO dto);

        Task<bool> Delete(int id);
    }
}
