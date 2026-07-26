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

            var department = await context.Departments.FirstOrDefaultAsync(x => x.Name == "Engineering");
            if (department == null)
            {
                department = new Department { Name = "Engineering" };
                context.Departments.Add(department);
                await context.SaveChangesAsync();
            }

            var admin = await EnsureUserAsync(context, passwordHasher, "admin", "admin@example.com", "Admin123!", "Admin");
            var manager = await EnsureUserAsync(context, passwordHasher, "manager", "manager@example.com", "Manager123!", "Manager");
            var employeeUser = await EnsureUserAsync(context, passwordHasher, "employee", "employee@example.com", "Employee123!", "Employee");

            var adminEmployee = await EnsureEmployeeAsync(context, department.Id, admin, "System Admin", "Administrator", 0);
            var managerEmployee = await EnsureEmployeeAsync(context, department.Id, manager, "Team Manager", "Manager", 5000);
            var employee = await EnsureEmployeeAsync(context, department.Id, employeeUser, "Test Employee", "Developer", 3000);

            var year = DateTime.UtcNow.Year;
            await EnsureBalanceAsync(context, adminEmployee.Id, year);
            await EnsureBalanceAsync(context, managerEmployee.Id, year);
            await EnsureBalanceAsync(context, employee.Id, year);
        }

        private static async Task<User> EnsureUserAsync(
            AppDbContext context,
            IPasswordHasher passwordHasher,
            string username,
            string email,
            string password,
            string role)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                return user;
            }

            user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHasher.Hash(password),
                Role = role
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        private static async Task<Employee> EnsureEmployeeAsync(
            AppDbContext context,
            int departmentId,
            User user,
            string fullName,
            string position,
            decimal salary)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (employee != null)
            {
                return employee;
            }

            employee = new Employee
            {
                FullName = fullName,
                Email = user.Email,
                Position = position,
                Salary = salary,
                DepartmentId = departmentId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            return employee;
        }

        private static async Task EnsureBalanceAsync(AppDbContext context, int employeeId, int year)
        {
            var exists = await context.LeaveBalances.AnyAsync(x => x.EmployeeId == employeeId && x.Year == year);
            if (exists)
            {
                return;
            }

            context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employeeId,
                Year = year,
                AnnualAllowance = DefaultAnnualAllowance,
                SickAllowance = DefaultSickAllowance
            });

            await context.SaveChangesAsync();
        }
    }
}
