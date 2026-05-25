using EmployeeManagement.API.DTOs.DepartmentDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> Create(DepartmentDTO dto) {
            var result = await _departmentService.Create(dto);

            return Ok(result);
        }

    }
}
