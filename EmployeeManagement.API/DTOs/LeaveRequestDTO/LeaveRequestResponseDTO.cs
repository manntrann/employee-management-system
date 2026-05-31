using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.DTOs.LeaveRequestDTO
{
    public class LeaveRequestResponseDTO
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        
        public LeaveType LeaveType { get; set; }
        
        public DateOnly StartDate { get; set; }
        
        public DateOnly EndDate { get; set; }
        
        public string? Reason { get; set; }
        
        public LeaveRequestStatus Status { get; set; }
        
        public DateTime RequestedAt { get; set; }
        
        public int? ReviewedByUserId { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        public string? ReviewNote { get; set; }
    }
}

