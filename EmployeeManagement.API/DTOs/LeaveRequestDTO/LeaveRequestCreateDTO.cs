using EmployeeManagement.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.LeaveRequestDTO
{
    public class LeaveRequestCreateDTO
    {
        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}

