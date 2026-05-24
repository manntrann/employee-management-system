using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeResponseDTO>> GetAll()
        {
            return await _context.Employees
            .Include(x => x.Department)
            .Select(x => new EmployeeResponseDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Position = x.Position,
                Salary = x.Salary,
                Phone = x.Phone,
                DepartmentName = x.Department.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        }

        public async Task<EmployeeResponseDTO?> GetById(int id)
        {
            return await _context.Employees
            .Include(x => x.Department)
            .Where(x => x.Id == id)
            .Select(x => new EmployeeResponseDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Position = x.Position,
                Salary = x.Salary,
                Phone = x.Phone,
                DepartmentName = x.Department.Name,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
        }

        public async Task<Employee> Create(EmployeeDTO dto)
        {
            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Position = dto.Position,
                Salary = dto.Salary,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };
            var departmentExists = await _context.Departments.AnyAsync(x => x.Id == dto.DepartmentId);

            if (!departmentExists)
            {
                return null;
            }
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

            if (!departmentExists)
            {
                return false;
            }
            employee.FullName = dto.FullName;
            employee.Email = dto.Email;
            employee.Position = dto.Position;
            employee.Salary = dto.Salary;
            employee.Phone = dto.Phone;
            employee.DepartmentId = dto.DepartmentId;

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
