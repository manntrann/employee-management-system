using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using EmployeeManagement.API.Services.Results;
using Xunit;

namespace EmployeeManagement.API.Tests.Services
{
    public class DepartmentServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task Delete_ReturnsHasEmployees_WhenDepartmentHasEmployees()
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

            var service = new DepartmentService(context);

            var result = await service.Delete(department.Id);

            Assert.Equal(DepartmentDeleteResult.HasEmployees, result);
        }
    }
}
