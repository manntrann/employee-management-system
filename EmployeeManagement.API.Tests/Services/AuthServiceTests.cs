using EmployeeManagement.API.DTOs.LoginDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using EmployeeManagement.API.Services.Interfaces;
using Xunit;

namespace EmployeeManagement.API.Tests.Services
{
    public class AuthServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task LoginAsync_ReturnsToken_ForValidCredentials()
        {
            await using var context = CreateContext();
            var passwordHasher = new PasswordHasher();

            context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = "Admin"
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, new StubJwtService(), passwordHasher);

            var token = await service.LoginAsync(new LoginDTO
            {
                Email = "admin@example.com",
                Password = "Admin123!"
            });

            Assert.Equal("test-token", token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_ForInvalidCredentials()
        {
            await using var context = CreateContext();
            var passwordHasher = new PasswordHasher();

            context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = "Admin"
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, new StubJwtService(), passwordHasher);

            var token = await service.LoginAsync(new LoginDTO
            {
                Email = "admin@example.com",
                Password = "wrong-password"
            });

            Assert.Null(token);
        }

        private class StubJwtService : IJwtService
        {
            public string GenerateToken(int userId, string email, string role)
            {
                return "test-token";
            }

            public System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
            {
                return null;
            }
        }
    }
}
