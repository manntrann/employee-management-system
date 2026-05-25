namespace EmployeeManagement.API.Models
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; } = new();
    }
}
