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
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(AppDbContext context, IJwtService jwtService, IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string?> LoginAsync(LoginDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email != null && x.Email == request.Email);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            return _jwtService.GenerateToken(
                user.Id,
                user.Email,
                user.Role
            );
        }
    }
}
