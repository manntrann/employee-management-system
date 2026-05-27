using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.UserDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserResponseDTO>> GetAll()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(user => new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                })
                .ToListAsync();
        }

        public async Task<UserResponseDTO?> GetById(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(user => user.Id == id)
                .Select(user => new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserResponseDTO> Create(UserDTO dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<bool> Update(int id, UserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return false;
            }

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.PasswordHash = _passwordHasher.Hash(dto.Password);
            user.Role = dto.Role;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserDeleteResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return UserDeleteResult.NotFound;
            }

            var hasEmployees = await _context.Employees.AnyAsync(e => e.UserId == id);

            if (hasEmployees)
            {
                return UserDeleteResult.HasEmployees;
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return UserDeleteResult.Deleted;
        }
    }
}
