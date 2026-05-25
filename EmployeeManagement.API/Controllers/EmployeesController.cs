using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(int page, int pageSize, string? search)
        {
            var employees = await _employeeService.GetAll(page, pageSize, search);

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById( int id )
        {
            var employee = await _employeeService.GetById( id );

            if(employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(EmployeeDTO dto)
        {
            var employee = await _employeeService.Create( dto );

            if (employee == null)
            {
                return NotFound("Department not found.");
            }   

            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                employee);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee (int id, EmployeeDTO dto)
        {
            var updated  = await _employeeService.Update( id, dto );

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
           
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var deleted = await _employeeService.Delete( id );

            if(!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
