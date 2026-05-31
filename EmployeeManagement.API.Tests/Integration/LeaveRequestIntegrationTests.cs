using EmployeeManagement.API.DTOs.LeaveRequestDTO;
using EmployeeManagement.API.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EmployeeManagement.API.Tests.Integration
{
    public class LeaveRequestIntegrationTests : IClassFixture<EmployeeApiFactory>
    {
        private readonly EmployeeApiFactory _factory;

        public LeaveRequestIntegrationTests(EmployeeApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Employee_CanCreateLeaveRequest_AndSeeItInMine()
        {
            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var createRes = await client.PostAsJsonAsync("/api/leave-requests", new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 2),
                Reason = "Vacation"
            });

            Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);

            var mineRes = await client.GetAsync("/api/leave-requests/mine");
            Assert.Equal(HttpStatusCode.OK, mineRes.StatusCode);

            var mine = await mineRes.Content.ReadFromJsonAsync<List<LeaveRequestResponseDTO>>();
            Assert.NotNull(mine);
            Assert.NotEmpty(mine!);
        }

        [Fact]
        public async Task Employee_CannotAccess_AdminLeaveRequestList()
        {
            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var res = await client.GetAsync("/api/leave-requests");

            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Fact]
        public async Task Employee_CannotApproveLeaveRequest()
        {
            var leaveRequestId = await CreateLeaveRequestAsync(
                "employee@example.com",
                "Employee123!",
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 2));

            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var res = await client.PostAsJsonAsync(
                $"/api/leave-requests/{leaveRequestId}/approve",
                new LeaveRequestReviewDTO { Note = "Self approve" });

            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenEndDateBeforeStartDate()
        {
            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var res = await client.PostAsJsonAsync("/api/leave-requests", new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 11, 10),
                EndDate = new DateOnly(2026, 11, 1)
            });

            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenDatesOverlap()
        {
            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var first = await client.PostAsJsonAsync("/api/leave-requests", new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 12, 1),
                EndDate = new DateOnly(2026, 12, 5)
            });
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);

            var second = await client.PostAsJsonAsync("/api/leave-requests", new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 12, 3),
                EndDate = new DateOnly(2026, 12, 7)
            });

            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task Employee_CanCancelPendingLeaveRequest()
        {
            var leaveRequestId = await CreateLeaveRequestAsync(
                "employee@example.com",
                "Employee123!",
                new DateOnly(2027, 1, 1),
                new DateOnly(2027, 1, 1));

            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var res = await client.PostAsync($"/api/leave-requests/{leaveRequestId}/cancel", null);

            Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        }

        [Fact]
        public async Task Admin_CanApproveLeaveRequest()
        {
            var leaveRequestId = await CreateLeaveRequestAsync(
                "employee@example.com",
                "Employee123!",
                new DateOnly(2027, 2, 1),
                new DateOnly(2027, 2, 2));

            var adminClient = await CreateAuthenticatedClientAsync("admin@example.com", "Admin123!");

            var approveRes = await adminClient.PostAsJsonAsync(
                $"/api/leave-requests/{leaveRequestId}/approve",
                new LeaveRequestReviewDTO { Note = "Enjoy your time off" });

            Assert.Equal(HttpStatusCode.NoContent, approveRes.StatusCode);

            var listRes = await adminClient.GetAsync("/api/leave-requests");
            listRes.EnsureSuccessStatusCode();

            var items = await listRes.Content.ReadFromJsonAsync<List<LeaveRequestResponseDTO>>();
            var approved = Assert.Single(items!, x => x.Id == leaveRequestId);

            Assert.Equal(LeaveRequestStatus.Approved, approved.Status);
            Assert.Equal("Enjoy your time off", approved.ReviewNote);
        }

        [Fact]
        public async Task Manager_CanApproveLeaveRequest()
        {
            var leaveRequestId = await CreateLeaveRequestAsync(
                "employee@example.com",
                "Employee123!",
                new DateOnly(2027, 3, 1),
                new DateOnly(2027, 3, 2));

            var managerClient = await CreateAuthenticatedClientAsync("manager@example.com", "Manager123!");

            var approveRes = await managerClient.PostAsJsonAsync(
                $"/api/leave-requests/{leaveRequestId}/approve",
                new LeaveRequestReviewDTO { Note = "Approved by manager" });

            Assert.Equal(HttpStatusCode.NoContent, approveRes.StatusCode);
        }

        [Fact]
        public async Task Admin_CanRejectLeaveRequest()
        {
            var leaveRequestId = await CreateLeaveRequestAsync(
                "employee@example.com",
                "Employee123!",
                new DateOnly(2027, 4, 1),
                new DateOnly(2027, 4, 2));

            var adminClient = await CreateAuthenticatedClientAsync("admin@example.com", "Admin123!");

            var rejectRes = await adminClient.PostAsJsonAsync(
                $"/api/leave-requests/{leaveRequestId}/reject",
                new LeaveRequestReviewDTO { Note = "Team is understaffed" });

            Assert.Equal(HttpStatusCode.NoContent, rejectRes.StatusCode);

            var listRes = await adminClient.GetAsync("/api/leave-requests");
            listRes.EnsureSuccessStatusCode();

            var items = await listRes.Content.ReadFromJsonAsync<List<LeaveRequestResponseDTO>>();
            var rejected = Assert.Single(items!, x => x.Id == leaveRequestId);

            Assert.Equal(LeaveRequestStatus.Rejected, rejected.Status);
            Assert.Equal("Team is understaffed", rejected.ReviewNote);
        }

        [Fact]
        public async Task Employee_CanGetLeaveBalance()
        {
            var client = await CreateAuthenticatedClientAsync("employee@example.com", "Employee123!");

            var res = await client.GetAsync("/api/leave-requests/balance");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var balance = await res.Content.ReadFromJsonAsync<LeaveBalanceResponseDTO>();
            Assert.NotNull(balance);
            Assert.Equal(12, balance!.AnnualAllowance);
            Assert.Equal(5, balance.SickAllowance);
        }

        private async Task<int> CreateLeaveRequestAsync(
            string email,
            string password,
            DateOnly startDate,
            DateOnly endDate)
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var createRes = await client.PostAsJsonAsync("/api/leave-requests", new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Sick,
                StartDate = startDate,
                EndDate = endDate,
                Reason = "Medical"
            });

            createRes.EnsureSuccessStatusCode();

            var created = await createRes.Content.ReadFromJsonAsync<LeaveRequestResponseDTO>();
            Assert.NotNull(created);

            return created!.Id;
        }

        private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
        {
            var client = _factory.CreateClient();
            var token = await LoginAndGetToken(client, email, password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<string> LoginAndGetToken(HttpClient client, string email, string password)
        {
            var res = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
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
