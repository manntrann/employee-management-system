using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs.LeaveRequestDTO;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private const decimal DefaultAnnualAllowance = 12;
        private const decimal DefaultSickAllowance = 5;

        private readonly AppDbContext _context;

        public LeaveRequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequestCreateOutcome> CreateForUserAsync(int userId, LeaveRequestCreateDTO dto)
        {
            if (dto.EndDate < dto.StartDate)
            {
                return LeaveRequestCreateOutcome.Fail(LeaveRequestCreateResult.InvalidDateRange);
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
            {
                return LeaveRequestCreateOutcome.Fail(LeaveRequestCreateResult.NoEmployeeProfile);
            }

            var startDate = ToUtcDate(dto.StartDate);
            var endDate = ToUtcDate(dto.EndDate);
            var requestedDays = CalculateLeaveDays(dto.StartDate, dto.EndDate);

            if (await HasOverlappingLeaveAsync(employee.Id, startDate, endDate))
            {
                return LeaveRequestCreateOutcome.Fail(LeaveRequestCreateResult.OverlappingLeave);
            }

            if (dto.LeaveType != LeaveType.Unpaid)
            {
                var balance = await GetOrCreateBalanceAsync(employee.Id, DateTime.UtcNow.Year);
                if (!HasSufficientBalance(balance, dto.LeaveType, requestedDays))
                {
                    return LeaveRequestCreateOutcome.Fail(LeaveRequestCreateResult.InsufficientBalance);
                }
            }

            var entity = new LeaveRequest
            {
                EmployeeId = employee.Id,
                LeaveType = dto.LeaveType,
                StartDate = startDate,
                EndDate = endDate,
                Reason = dto.Reason,
                Status = LeaveRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(entity);
            await _context.SaveChangesAsync();

            return LeaveRequestCreateOutcome.Success(Map(entity));
        }

        public async Task<List<LeaveRequestResponseDTO>> GetMineAsync(int userId)
        {
            var employeeId = await _context.Employees
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (employeeId == null)
            {
                return new List<LeaveRequestResponseDTO>();
            }

            var items = await _context.LeaveRequests
                .AsNoTracking()
                .Where(lr => lr.EmployeeId == employeeId.Value)
                .OrderByDescending(lr => lr.RequestedAt)
                .ToListAsync();

            return items.Select(Map).ToList();
        }

        public async Task<List<LeaveRequestResponseDTO>> GetAllAsync()
        {
            var items = await _context.LeaveRequests
                .AsNoTracking()
                .OrderByDescending(lr => lr.RequestedAt)
                .ToListAsync();

            return items.Select(Map).ToList();
        }

        public async Task<LeaveBalanceResponseDTO?> GetBalanceForUserAsync(int userId)
        {
            var employeeId = await _context.Employees
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (employeeId == null)
            {
                return null;
            }

            var balance = await GetOrCreateBalanceAsync(employeeId.Value, DateTime.UtcNow.Year);
            return MapBalance(balance);
        }

        public async Task<LeaveRequestCancelResult> CancelMineAsync(int userId, int leaveRequestId)
        {
            var entity = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (entity == null)
            {
                return LeaveRequestCancelResult.NotFound;
            }

            if (entity.Employee.UserId != userId)
            {
                return LeaveRequestCancelResult.NotOwner;
            }

            if (entity.Status != LeaveRequestStatus.Pending)
            {
                return LeaveRequestCancelResult.NotPending;
            }

            entity.Status = LeaveRequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            return LeaveRequestCancelResult.Success;
        }

        public async Task<LeaveRequestReviewResult> ApproveAsync(int reviewerUserId, int leaveRequestId, string? note)
        {
            var entity = await _context.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (entity == null)
            {
                return LeaveRequestReviewResult.NotFound;
            }

            if (entity.Status != LeaveRequestStatus.Pending)
            {
                return LeaveRequestReviewResult.NotPending;
            }

            var days = CalculateLeaveDays(
                DateOnly.FromDateTime(entity.StartDate),
                DateOnly.FromDateTime(entity.EndDate));

            if (entity.LeaveType != LeaveType.Unpaid)
            {
                var year = entity.StartDate.Year;
                var balance = await GetOrCreateBalanceAsync(entity.EmployeeId, year);

                if (!HasSufficientBalance(balance, entity.LeaveType, days))
                {
                    return LeaveRequestReviewResult.InsufficientBalance;
                }

                DeductBalance(balance, entity.LeaveType, days);
            }

            entity.Status = LeaveRequestStatus.Approved;
            entity.ReviewedByUserId = reviewerUserId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.ReviewNote = note;

            await _context.SaveChangesAsync();

            return LeaveRequestReviewResult.Success;
        }

        public async Task<LeaveRequestReviewResult> RejectAsync(int reviewerUserId, int leaveRequestId, string? note)
        {
            var entity = await _context.LeaveRequests.FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (entity == null)
            {
                return LeaveRequestReviewResult.NotFound;
            }

            if (entity.Status != LeaveRequestStatus.Pending)
            {
                return LeaveRequestReviewResult.NotPending;
            }

            entity.Status = LeaveRequestStatus.Rejected;
            entity.ReviewedByUserId = reviewerUserId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.ReviewNote = note;

            await _context.SaveChangesAsync();

            return LeaveRequestReviewResult.Success;
        }

        private async Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.LeaveRequests.AnyAsync(lr =>
                lr.EmployeeId == employeeId &&
                (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved) &&
                lr.StartDate <= endDate &&
                lr.EndDate >= startDate);
        }

        private async Task<LeaveBalance> GetOrCreateBalanceAsync(int employeeId, int year)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Year == year);

            if (balance != null)
            {
                return balance;
            }

            balance = new LeaveBalance
            {
                EmployeeId = employeeId,
                Year = year,
                AnnualAllowance = DefaultAnnualAllowance,
                SickAllowance = DefaultSickAllowance
            };

            _context.LeaveBalances.Add(balance);
            await _context.SaveChangesAsync();

            return balance;
        }

        private static bool HasSufficientBalance(LeaveBalance balance, LeaveType leaveType, decimal requestedDays)
        {
            return leaveType switch
            {
                LeaveType.Annual => balance.AnnualAllowance - balance.AnnualUsed >= requestedDays,
                LeaveType.Sick => balance.SickAllowance - balance.SickUsed >= requestedDays,
                _ => true
            };
        }

        private static void DeductBalance(LeaveBalance balance, LeaveType leaveType, decimal days)
        {
            switch (leaveType)
            {
                case LeaveType.Annual:
                    balance.AnnualUsed += days;
                    break;
                case LeaveType.Sick:
                    balance.SickUsed += days;
                    break;
            }
        }

        private static int CalculateLeaveDays(DateOnly startDate, DateOnly endDate) =>
            endDate.DayNumber - startDate.DayNumber + 1;

        private static DateTime ToUtcDate(DateOnly date) =>
            date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        private static LeaveBalanceResponseDTO MapBalance(LeaveBalance balance) =>
            new()
            {
                Year = balance.Year,
                AnnualAllowance = balance.AnnualAllowance,
                AnnualUsed = balance.AnnualUsed,
                AnnualRemaining = balance.AnnualAllowance - balance.AnnualUsed,
                SickAllowance = balance.SickAllowance,
                SickUsed = balance.SickUsed,
                SickRemaining = balance.SickAllowance - balance.SickUsed
            };

        private static LeaveRequestResponseDTO Map(LeaveRequest entity) =>
            new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                LeaveType = entity.LeaveType,
                StartDate = DateOnly.FromDateTime(entity.StartDate),
                EndDate = DateOnly.FromDateTime(entity.EndDate),
                Reason = entity.Reason,
                Status = entity.Status,
                RequestedAt = entity.RequestedAt,
                ReviewedByUserId = entity.ReviewedByUserId,
                ReviewedAt = entity.ReviewedAt,
                ReviewNote = entity.ReviewNote
            };
    }
}
