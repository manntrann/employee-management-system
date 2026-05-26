using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.DepartmentDTO;
using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentResponseDTO>> GetAll()
        {
            return await _context.Departments.Include(e => e.Employees).Select(d => new DepartmentResponseDTO
            {
                Id = d.Id,
                DepartmentName = d.Name,
                Employee = d.Employees.Select(e => new EmployeeResponseDTO
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Position = e.Position,
                    Salary = e.Salary,
                    Phone = e.Phone,
                    DepartmentName = d.Name,
                    CreatedAt = e.CreatedAt
                }).ToList()

            }).ToListAsync();
        }

        public async Task<DepartmentResponseDTO?> GetById(int id)
        {
            return await _context.Departments
                .Include(e => e.Employees)
                .Where(d => d.Id == id)
                .Select(d => new DepartmentResponseDTO
                {
                    Id = d.Id,
                    DepartmentName = d.Name,
                    Employee = d.Employees.Select(e => new EmployeeResponseDTO
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        Email = e.Email,
                        Position = e.Position,
                        Salary = e.Salary,
                        Phone = e.Phone,
                        DepartmentName = d.Name,
                        CreatedAt = e.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Department> Create(DepartmentDTO dto)
        {
            var department = new Department
            {
                Name = dto.Name
            };

            _context.Departments.Add(department);

            await _context.SaveChangesAsync();

            return department;
        }

        public async Task<bool> Update(int id, DepartmentDTO dto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return false;
            }

            department.Name = dto.Name;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DepartmentDeleteResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return DepartmentDeleteResult.NotFound;
            }

            var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id);

            if (hasEmployees)
            {
                return DepartmentDeleteResult.HasEmployees;
            }

            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();

            return DepartmentDeleteResult.Deleted;
        }
    }
}
