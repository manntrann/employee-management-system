using System.Security.Claims;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
