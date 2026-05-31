namespace EmployeeManagement.API.Models
{
    public class LeaveBalance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public int Year { get; set; }

        public decimal AnnualAllowance { get; set; } = 12;

        public decimal SickAllowance { get; set; } = 5;

        public decimal AnnualUsed { get; set; }

        public decimal SickUsed { get; set; }
    }
}
