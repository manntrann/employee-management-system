using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.LeaveRequestDTO
{
    public class LeaveRequestReviewDTO
    {
        [StringLength(500)]
        public string? Note { get; set; }
    }
}

