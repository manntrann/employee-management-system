using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.UserDTO;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetById(id);

            if (user == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "User not found." });
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserDTO dto)
        {
            var user = await _userService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserDTO dto)
        {
            var updated = await _userService.Update(id, dto);

            if (!updated)
            {
                return NotFound(new ErrorResponseDTO { Message = "User not found." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.Delete(id);

            if (result == UserDeleteResult.NotFound)
            {
                return NotFound(new ErrorResponseDTO { Message = "User not found." });
            }

            if (result == UserDeleteResult.HasEmployees)
            {
                return Conflict(new ErrorResponseDTO
                {
                    Message = "Cannot delete user because employees are still assigned to this user. Reassign or delete those employees first."
                });
            }

            return NoContent();
        }
    }
}
