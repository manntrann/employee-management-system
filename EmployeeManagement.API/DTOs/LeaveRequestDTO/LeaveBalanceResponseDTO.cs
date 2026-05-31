namespace EmployeeManagement.API.DTOs.LeaveRequestDTO
{
    public class LeaveBalanceResponseDTO
    {
        public int Year { get; set; }

        public decimal AnnualAllowance { get; set; }

        public decimal AnnualUsed { get; set; }

        public decimal AnnualRemaining { get; set; }

        public decimal SickAllowance { get; set; }

        public decimal SickUsed { get; set; }

        public decimal SickRemaining { get; set; }
    }
}
