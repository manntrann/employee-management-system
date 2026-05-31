using EmployeeManagement.API.DTOs.LeaveRequestDTO;

namespace EmployeeManagement.API.Services.Results
{
    public class LeaveRequestCreateOutcome
    {
        public LeaveRequestCreateResult Result { get; init; }

        public LeaveRequestResponseDTO? LeaveRequest { get; init; }

        public static LeaveRequestCreateOutcome Fail(LeaveRequestCreateResult result) =>
            new() { Result = result };

        public static LeaveRequestCreateOutcome Success(LeaveRequestResponseDTO leaveRequest) =>
            new() { Result = LeaveRequestCreateResult.Created, LeaveRequest = leaveRequest };
    }
}
