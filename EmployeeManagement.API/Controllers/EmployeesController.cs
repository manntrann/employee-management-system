using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search)
        {
            var employees = await _employeeService.GetAll(page ?? 1, pageSize ?? 10, search);

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetById(id);

            if (employee == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee not found." });
            }

            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(EmployeeDTO dto)
        {
            var employee = await _employeeService.Create(dto);

            if (employee == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Department or user not found." });
            }

            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDTO dto)
        {
            var updated = await _employeeService.Update(id, dto);

            if (!updated)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee, department, or user not found." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var deleted = await _employeeService.Delete(id);

            if (!deleted)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee not found." });
            }

            return NoContent();
        }
    }
}
