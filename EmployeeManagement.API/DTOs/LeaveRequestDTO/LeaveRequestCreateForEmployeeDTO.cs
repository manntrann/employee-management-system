using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.LeaveRequestDTO
{
    public class LeaveRequestCreateForEmployeeDTO : LeaveRequestCreateDTO
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }
    }
}
