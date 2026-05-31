using EmployeeManagement.API.DTOs.LeaveRequestDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using EmployeeManagement.API.Services.Results;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeManagement.API.Tests.Services
{
    public class LeaveRequestServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task ApproveAsync_SetsApproved_AndDeductsAnnualBalance()
        {
            await using var context = CreateContext();
            var (adminUserId, employeeId, leaveRequestId) = await SeedPendingLeaveRequestAsync(context);

            var service = new LeaveRequestService(context);

            var result = await service.ApproveAsync(adminUserId, leaveRequestId, "Approved for vacation");

            Assert.Equal(LeaveRequestReviewResult.Success, result);

            var entity = await context.LeaveRequests.FindAsync(leaveRequestId);
            Assert.NotNull(entity);
            Assert.Equal(LeaveRequestStatus.Approved, entity!.Status);

            var balance = await context.LeaveBalances.SingleAsync(b => b.EmployeeId == employeeId);
            Assert.Equal(2, balance.AnnualUsed);
        }

        [Fact]
        public async Task RejectAsync_SetsRejected_WhenPending()
        {
            await using var context = CreateContext();
            var (adminUserId, _, leaveRequestId) = await SeedPendingLeaveRequestAsync(context);

            var service = new LeaveRequestService(context);

            var result = await service.RejectAsync(adminUserId, leaveRequestId, "Not enough coverage");

            Assert.Equal(LeaveRequestReviewResult.Success, result);

            var entity = await context.LeaveRequests.FindAsync(leaveRequestId);
            Assert.NotNull(entity);
            Assert.Equal(LeaveRequestStatus.Rejected, entity!.Status);
        }

        [Fact]
        public async Task ApproveAsync_ReturnsNotPending_WhenAlreadyApproved()
        {
            await using var context = CreateContext();
            var (adminUserId, _, leaveRequestId) = await SeedPendingLeaveRequestAsync(context);

            var service = new LeaveRequestService(context);
            await service.ApproveAsync(adminUserId, leaveRequestId, null);

            var result = await service.ApproveAsync(adminUserId, leaveRequestId, null);

            Assert.Equal(LeaveRequestReviewResult.NotPending, result);
        }

        [Fact]
        public async Task CreateForUserAsync_ReturnsInvalidDateRange_WhenEndBeforeStart()
        {
            await using var context = CreateContext();
            var (_, employeeUserId, _) = await SeedEmployeeWithBalanceAsync(context);

            var service = new LeaveRequestService(context);

            var outcome = await service.CreateForUserAsync(employeeUserId, new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 6, 10),
                EndDate = new DateOnly(2026, 6, 1)
            });

            Assert.Equal(LeaveRequestCreateResult.InvalidDateRange, outcome.Result);
        }

        [Fact]
        public async Task CreateForUserAsync_ReturnsOverlappingLeave_WhenDatesOverlap()
        {
            await using var context = CreateContext();
            var (_, employeeUserId, _) = await SeedEmployeeWithBalanceAsync(context);

            var service = new LeaveRequestService(context);

            await service.CreateForUserAsync(employeeUserId, new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 8, 1),
                EndDate = new DateOnly(2026, 8, 5)
            });

            var outcome = await service.CreateForUserAsync(employeeUserId, new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Annual,
                StartDate = new DateOnly(2026, 8, 3),
                EndDate = new DateOnly(2026, 8, 7)
            });

            Assert.Equal(LeaveRequestCreateResult.OverlappingLeave, outcome.Result);
        }

        [Fact]
        public async Task CancelMineAsync_CancelsPendingRequest()
        {
            await using var context = CreateContext();
            var (_, employeeUserId, _) = await SeedEmployeeWithBalanceAsync(context);

            var service = new LeaveRequestService(context);
            var created = await service.CreateForUserAsync(employeeUserId, new LeaveRequestCreateDTO
            {
                LeaveType = LeaveType.Sick,
                StartDate = new DateOnly(2026, 9, 1),
                EndDate = new DateOnly(2026, 9, 1)
            });

            var result = await service.CancelMineAsync(employeeUserId, created.LeaveRequest!.Id);

            Assert.Equal(LeaveRequestCancelResult.Success, result);

            var entity = await context.LeaveRequests.FindAsync(created.LeaveRequest.Id);
            Assert.Equal(LeaveRequestStatus.Cancelled, entity!.Status);
        }

        private static async Task<(int adminUserId, int employeeId, int leaveRequestId)> SeedPendingLeaveRequestAsync(
            Data.AppDbContext context)
        {
            var (adminUserId, employeeUserId, employeeId) = await SeedEmployeeWithBalanceAsync(context);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveType = LeaveType.Annual,
                StartDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc),
                Status = LeaveRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };
            context.LeaveRequests.Add(leaveRequest);
            await context.SaveChangesAsync();

            return (adminUserId, employeeId, leaveRequest.Id);
        }

        private static async Task<(int adminUserId, int employeeUserId, int employeeId)> SeedEmployeeWithBalanceAsync(
            Data.AppDbContext context)
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = "Admin"
            };
            var employeeUser = new User
            {
                Username = "emp",
                Email = "emp@example.com",
                PasswordHash = "hash",
                Role = "Employee"
            };
            var department = new Department { Name = "Engineering" };

            context.Users.AddRange(admin, employeeUser);
            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                FullName = "Jane Doe",
                Email = employeeUser.Email,
                DepartmentId = department.Id,
                UserId = employeeUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                Year = 2026,
                AnnualAllowance = 12,
                SickAllowance = 5
            });
            await context.SaveChangesAsync();

            return (admin.Id, employeeUser.Id, employee.Id);
        }
    }
}
