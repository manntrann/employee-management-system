using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.EmployeeDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EmployeeManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IJwtService _jwtService;

        public EmployeesController(IEmployeeService employeeService, IJwtService jwtService)
        {
            _employeeService = employeeService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search)
        {
            var employees = await _employeeService.GetAll(page ?? 1, pageSize ?? 10, search);

            return Ok(employees);
        }

        [HttpGet("myprofile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var employee = await _employeeService.GetByUserId(currentUserId.Value);
            if (employee == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee profile not found." });
            }

            return Ok(employee);
        }

        [HttpPut("myprofile")]
        public async Task<IActionResult> UpdateMyProfile(EmployeeProfileUpdateDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var employee = await _employeeService.UpdateProfile(currentUserId.Value, dto);
            if (employee == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee profile not found or email is already used." });
            }

            var token = _jwtService.GenerateToken(employee.UserId, employee.Email ?? string.Empty, employee.Role);
            return Ok(new { employee, token });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetById(id);

            if (employee == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee not found." });
            }

            return Ok(employee);
        }

        [Authorize(Roles = "Admin,Manager")]
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            var updated = await _employeeService.Update(id, dto);

            if (!updated)
            {
                return NotFound(new ErrorResponseDTO { Message = "Employee, department, or user not found." });
            }

            var employee = await _employeeService.GetById(id);
            if (employee != null && currentUserId == employee.UserId)
            {
                var token = _jwtService.GenerateToken(employee.UserId, employee.Email ?? string.Empty, employee.Role);
                return Ok(new { token });
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin,Manager")]
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

        private int? GetCurrentUserId()
        {
            var idValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return int.TryParse(idValue, out var id) ? id : null;
        }
    }
}
