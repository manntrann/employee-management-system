using EmployeeManagement.API.DTOs.UserDTO;
using EmployeeManagement.API.Services.Results;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAll();

        Task<UserResponseDTO?> GetById(int id);

        Task<UserResponseDTO> Create(UserDTO dto);

        Task<bool> Update(int id, UserDTO dto);

        Task<UserDeleteResult> Delete(int id);
    }
}
