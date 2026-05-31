namespace EmployeeManagement.API.Services.Results
{
    public enum LeaveRequestCancelResult
    {
        Success,
        NotFound,
        NotOwner,
        NotPending
    }
}
