using EmployeeManagement.API.DTOs.LoginDTO;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDTO request);
    }
}
