namespace EmployeeManagement.API.Services.Results
{
    public enum LeaveRequestCreateResult
    {
        Created,
        InvalidDateRange,
        NoEmployeeProfile,
        OverlappingLeave,
        InsufficientBalance
    }
}
