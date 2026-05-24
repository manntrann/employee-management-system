using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class EmployeesController : Controller
    {
        private readonly EmployeeService _service;

        public EmployeesController(EmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _service.GetAll();

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById( int id )
        {
            var employee = await _service.GetById( id );

            if(employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(EmployeeDTO dto)
        {
            var employee = await _service.Create( dto );

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
            var updated  = await _service.Update( id, dto );

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
           
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var deleted = await _service.Delete( id );

            if(!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
