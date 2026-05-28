using EmployeeManagement.API.DTOs.UserDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using EmployeeManagement.API.Services.Results;
using Xunit;

namespace EmployeeManagement.API.Tests.Services
{
    public class UserServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task Create_HashesPassword()
        {
            await using var context = CreateContext();
            var passwordHasher = new PasswordHasher();
            var service = new UserService(context, passwordHasher);

            var user = await service.Create(new UserDTO
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "Admin123!",
                Role = "Admin"
            });

            var storedUser = await context.Users.FindAsync(user.Id);

            Assert.NotNull(storedUser);
            Assert.NotEqual("Admin123!", storedUser!.PasswordHash);
            Assert.True(passwordHasher.Verify("Admin123!", storedUser.PasswordHash));
        }

        [Fact]
        public async Task Delete_ReturnsHasEmployees_WhenUserHasEmployees()
        {
            await using var context = CreateContext();
            var department = new Department { Name = "Engineering" };
            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = "Admin"
            };

            context.Departments.Add(department);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Employees.Add(new Employee
            {
                FullName = "Jane Doe",
                DepartmentId = department.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new UserService(context, new PasswordHasher());

            var result = await service.Delete(user.Id);

            Assert.Equal(UserDeleteResult.HasEmployees, result);
        }
    }
}
