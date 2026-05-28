using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.DepartmentDTO;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAll();

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departmentService.GetById(id);

            if (department == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Department not found." });
            }

            return Ok(department);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DepartmentDTO dto)
        {
            var result = await _departmentService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DepartmentDTO dto)
        {
            var updated = await _departmentService.Update(id, dto);

            if (!updated)
            {
                return NotFound(new ErrorResponseDTO { Message = "Department not found." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _departmentService.Delete(id);

            if (result == DepartmentDeleteResult.NotFound)
            {
                return NotFound(new ErrorResponseDTO { Message = "Department not found." });
            }

            if (result == DepartmentDeleteResult.HasEmployees)
            {
                return Conflict(new ErrorResponseDTO
                {
                    Message = "Cannot delete department because it still has employees. Move or delete those employees first."
                });
            }

            return NoContent();
        }
    }
}
