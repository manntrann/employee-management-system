using EmployeeManagement.API.DTOs.LoginDTO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EmployeeManagement.API.Tests.Integration
{
    public class AuthRoleIntegrationTests : IClassFixture<EmployeeApiFactory>
    {
        private readonly EmployeeApiFactory _factory;

        public AuthRoleIntegrationTests(EmployeeApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminEndpoints_Return401_WithoutToken()
        {
            var client = _factory.CreateClient();

            var res = await client.GetAsync("/api/departments");

            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task AdminEndpoints_Return403_ForNonAdminToken()
        {
            var client = _factory.CreateClient();
            var token = await LoginAndGetToken(client, "employee@example.com", "Employee123!");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("/api/departments");

            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Fact]
        public async Task AdminEndpoints_Return200_ForAdminToken()
        {
            var client = _factory.CreateClient();
            var token = await LoginAndGetToken(client, "admin@example.com", "Admin123!");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("/api/departments");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsToken_ForValidCredentials()
        {
            var client = _factory.CreateClient();

            var token = await LoginAndGetToken(client, "admin@example.com", "Admin123!");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        private static async Task<string> LoginAndGetToken(HttpClient client, string email, string password)
        {
            var res = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
            {
                Email = email,
                Password = password
            });

            res.EnsureSuccessStatusCode();

            var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(payload);
            Assert.False(string.IsNullOrWhiteSpace(payload!.Token));

            return payload.Token;
        }

        private sealed class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}

