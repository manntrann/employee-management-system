using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public EmployeeService(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<object> GetAll(int page, int pageSize, string? search)
        {
            const int defaultPage = 1;
            const int defaultPageSize = 10;
            const int maxPageSize = 100;

            page = page <= 0 ? defaultPage : page;
            pageSize = pageSize <= 0 ? defaultPageSize : pageSize;
            pageSize = Math.Min(pageSize, maxPageSize);

            var query = _context.Employees
               .Include(x => x.Department)
               .Include(x => x.User)
               .AsNoTracking()
               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
               .Select(x => new EmployeeResponseDTO
               {
                   Id = x.Id,
                   FullName = x.FullName,
                   Email = x.Email,
                   Position = x.Position,
                   Salary = x.Salary,
                   Phone = x.Phone,
                   DepartmentId = x.DepartmentId,
                   DepartmentName = x.Department.Name,
                   UserId = x.UserId,
                   UserName = x.User.Username,
                   Role = x.User.Role,
                   CreatedAt = x.CreatedAt
               })
                .ToListAsync();

            return new { total, page, pageSize, data };
        }

        public async Task<EmployeeResponseDTO?> GetById(int id)
        {
            return await _context.Employees
            .Include(x => x.Department)
            .Include(x => x.User)
            .Where(x => x.Id == id)
            .Select(x => new EmployeeResponseDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Position = x.Position,
                Salary = x.Salary,
                Phone = x.Phone,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,
                UserId = x.UserId,
                UserName = x.User.Username,
                Role = x.User.Role,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
        }

        public async Task<Employee?> Create(EmployeeDTO dto)
        {
            var departmentExists = await _context.Departments.AnyAsync(x => x.Id == dto.DepartmentId);
            var email = dto.Email?.Trim();
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role.Trim();

            if (!departmentExists ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                await _context.Users.AnyAsync(x => x.Email == email))
            {
                return null;
            }

            var user = new User
            {
                Username = dto.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = email,
                Position = dto.Position,
                Salary = dto.Salary,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> Update(int id, EmployeeDTO dto)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return false;
            var departmentExists = await _context.Departments.AnyAsync(x => x.Id == dto.DepartmentId);
            var user = await _context.Users.FindAsync(dto.UserId ?? employee.UserId);

            if (!departmentExists || user == null)
            {
                return false;
            }

            var email = dto.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailUsed = await _context.Users.AnyAsync(x => x.Email == email && x.Id != user.Id);
                if (emailUsed)
                {
                    return false;
                }
            }

            employee.FullName = dto.FullName;
            employee.Email = email;
            employee.Position = dto.Position;
            employee.Salary = dto.Salary;
            employee.Phone = dto.Phone;
            employee.DepartmentId = dto.DepartmentId;
            employee.UserId = user.Id;

            user.Username = dto.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                user.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                user.Role = dto.Role.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = _passwordHasher.Hash(dto.Password);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return false;

            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
