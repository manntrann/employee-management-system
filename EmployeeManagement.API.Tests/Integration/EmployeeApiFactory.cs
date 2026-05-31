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

            context.Employees.Add(employee);
            context.SaveChanges();

            context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                Year = DateTime.UtcNow.Year,
                AnnualAllowance = 12,
                SickAllowance = 5
            });

            context.SaveChanges();
        }
    }
}