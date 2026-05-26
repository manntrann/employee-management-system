using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.LoginDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<string> LoginAsync(LoginDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    (x.Username != null && x.Username == request.UserName) &&
                    x.PasswordHash == request.Password);

            if (user == null)
            {
                return null;
            }

            return _jwtService.GenerateToken(
                user.Id,
                user.Username,
                user.Role
            );
        }
    }
}
