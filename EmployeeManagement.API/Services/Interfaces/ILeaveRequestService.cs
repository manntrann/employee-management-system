using EmployeeManagement.API.DTOs.LeaveRequestDTO;
using EmployeeManagement.API.Services.Results;

namespace EmployeeManagement.API.Services.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestCreateOutcome> CreateForUserAsync(int userId, LeaveRequestCreateDTO dto);

        Task<List<LeaveRequestResponseDTO>> GetMineAsync(int userId);

        Task<List<LeaveRequestResponseDTO>> GetAllAsync();

        Task<LeaveBalanceResponseDTO?> GetBalanceForUserAsync(int userId);

        Task<LeaveRequestCancelResult> CancelMineAsync(int userId, int leaveRequestId);

        Task<LeaveRequestReviewResult> ApproveAsync(int reviewerUserId, int leaveRequestId, string? note);

        Task<LeaveRequestReviewResult> RejectAsync(int reviewerUserId, int leaveRequestId, string? note);
    }
}
