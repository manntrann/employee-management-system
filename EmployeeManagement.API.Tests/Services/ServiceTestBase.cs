using EmployeeManagement.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Tests.Services
{
    public abstract class ServiceTestBase
    {
        protected static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
