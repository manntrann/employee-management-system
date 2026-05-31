using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models
{
    public enum LeaveRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }

    public enum LeaveType
    {
        Annual = 0,
        Sick = 1,
        Unpaid = 2
    }

    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public int? ReviewedByUserId { get; set; }

        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewNote { get; set; }
    }
}

