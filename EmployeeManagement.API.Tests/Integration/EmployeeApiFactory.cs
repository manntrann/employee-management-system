using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeManagement.API.Tests.Integration

{
    public class EmployeeApiFactory : WebApplicationFactory<Program>

    {
        private readonly InMemoryDatabaseRoot _dbRoot = new();

        private readonly string _dbName = "employee-management-integration-tests";

        protected override void ConfigureWebHost(IWebHostBuilder builder)

        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>

                {
                    ["Jwt:Key"] = "integration-test-super-secret-key-please-change",

                    ["Jwt:Issuer"] = "EmployeeManagement.API.Tests",

                    ["Jwt:Audience"] = "EmployeeManagement.API.Tests",

                    ["Jwt:DurationInMinutes"] = "60"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>

            {
                services.RemoveAll(typeof(AppDbContext));

                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                services.RemoveAll(typeof(DbContextOptions));

                services.AddDbContext<AppDbContext>(options =>

                {
                    options.UseInMemoryDatabase(_dbName, _dbRoot);

                });

                using var scope = services.BuildServiceProvider().CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                context.Database.EnsureCreated();

                SeedDevelopmentData(context, passwordHasher);
            });
        }

        private static void SeedDevelopmentData(AppDbContext context, IPasswordHasher passwordHasher)

        {
            if (context.Users.Any())
            {
                return;
            }

            var department = new Department { Name = "Engineering" };

            context.Departments.Add(department);

            var adminUser = new User

            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = "Admin"
            };

            var managerUser = new User
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

            context.Users.AddRange(adminUser, managerUser, employeeUser);
            context.SaveChanges();

            var adminEmployee = new Employee
            {
                FullName = "System Admin",
                Email = adminUser.Email,
                Position = "Administrator",
                Salary = 0,
                Phone = "0000000001",
                DepartmentId = department.Id,
                UserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var managerEmployee = new Employee
            {
                FullName = "Team Manager",
                Email = managerUser.Email,
                Position = "Manager",
                Salary = 5000,
                Phone = "0000000002",
                DepartmentId = department.Id,
                UserId = managerUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var employee = new Employee
            {
                FullName = "Test Employee",
                Email = employeeUser.Email,
                Position = "Developer",
                Salary = 1000,
                Phone = "0000000000",
                DepartmentId = department.Id,
                UserId = employeeUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.AddRange(adminEmployee, managerEmployee, employee);
            context.SaveChanges();

            var year = DateTime.UtcNow.Year;
            context.LeaveBalances.AddRange(
                CreateBalance(adminEmployee.Id, year),
                CreateBalance(managerEmployee.Id, year),
                CreateBalance(employee.Id, year));

            context.SaveChanges();
        }

        private static LeaveBalance CreateBalance(int employeeId, int year) =>
            new()
            {
                EmployeeId = employeeId,
                Year = year,
                AnnualAllowance = 12,
                SickAllowance = 5
            };
    }
}
