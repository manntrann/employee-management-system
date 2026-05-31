using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Data
{
    public static class DbSeeder
    {
        private const decimal DefaultAnnualAllowance = 12;
        private const decimal DefaultSickAllowance = 5;

        public static async Task SeedDevelopmentDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            if (await context.Users.AnyAsync())
            {
                return;
            }

            var department = new Department { Name = "Engineering" };
            context.Departments.Add(department);

            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = "Admin"
            };

            var manager = new User
            {
                Username = "manager",
                Email = "manager@example.com",
                PasswordHash = passwordHasher.Hash("Manager123!"),
                Role = "Manager"
            };

            var employeeUser = new User
            {
                Username = "employee",
                Email = "employee@example.com",
                PasswordHash = passwordHasher.Hash("Employee123!"),
                Role = "Employee"
            };

            context.Users.AddRange(admin, manager, employeeUser);
            await context.SaveChangesAsync();

            var adminEmployee = new Employee
            {
                FullName = "System Admin",
                Email = admin.Email,
                Position = "Administrator",
                Salary = 0,
                DepartmentId = department.Id,
                UserId = admin.Id,
                CreatedAt = DateTime.UtcNow
            };

            var managerEmployee = new Employee
            {
                FullName = "Team Manager",
                Email = manager.Email,
                Position = "Manager",
                Salary = 5000,
                DepartmentId = department.Id,
                UserId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var employee = new Employee
            {
                FullName = "Test Employee",
                Email = employeeUser.Email,
                Position = "Developer",
                Salary = 3000,
                DepartmentId = department.Id,
                UserId = employeeUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.AddRange(adminEmployee, managerEmployee, employee);
            await context.SaveChangesAsync();

            var year = DateTime.UtcNow.Year;
            context.LeaveBalances.AddRange(
                CreateBalance(adminEmployee.Id, year),
                CreateBalance(managerEmployee.Id, year),
                CreateBalance(employee.Id, year));

            await context.SaveChangesAsync();
        }

        private static LeaveBalance CreateBalance(int employeeId, int year) =>
            new()
            {
                EmployeeId = employeeId,
                Year = year,
                AnnualAllowance = DefaultAnnualAllowance,
                SickAllowance = DefaultSickAllowance
            };
    }
}
