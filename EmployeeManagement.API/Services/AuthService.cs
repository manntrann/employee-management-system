using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.LoginDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EmployeeManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        public AuthService(
            AppDbContext context,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<string?> LoginAsync(LoginDTO request)
        {
            var employee = await _context.Employees
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Email != null && x.Email == request.Email);

            var user = employee?.User;

            if (user == null)
            {
                return null;
            }

            var validPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
            var validTemporaryPassword =
                !string.IsNullOrWhiteSpace(user.TemporaryPasswordHash) &&
                _passwordHasher.Verify(request.Password, user.TemporaryPasswordHash);

            if (!validPassword && !validTemporaryPassword)
            {
                return null;
            }

            if (validTemporaryPassword)
            {
                user.TemporaryPasswordHash = null;
                await _context.SaveChangesAsync();
            }

            return _jwtService.GenerateToken(
                user.Id,
                user.Email,
                user.Role
            );
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDTO request)
        {
            var email = request.Email.Trim();
            var employee = await _context.Employees
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Email != null && x.Email == email);

            var user = employee?.User;
            if (user == null)
            {
                return;
            }

            var temporaryPassword = GenerateTemporaryPassword();
            var body = $"""
                Hello {employee!.FullName},

                Your PeopleHub password has been reset.

                Temporary password: {temporaryPassword}

                Please sign in and change your password after logging in.
                """;

            await _emailService.SendAsync(user.Email, "PeopleHub password reset", body);

            user.TemporaryPasswordHash = _passwordHasher.Hash(temporaryPassword);
            await _context.SaveChangesAsync();
        }

        private static string GenerateTemporaryPassword()
        {
            const string characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$?";
            var bytes = RandomNumberGenerator.GetBytes(14);

            return new string(bytes.Select(value => characters[value % characters.Length]).ToArray());
        }
    }
}
