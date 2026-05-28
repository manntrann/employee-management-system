using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            if (await context.Users.AnyAsync(x => x.Role == "Admin"))
            {
                return;
            }

            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = "Admin"
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
