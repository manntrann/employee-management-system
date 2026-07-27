using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.LoginDTO;
using EmployeeManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var token = await _authService.LoginAsync(request);

            if (token == null)
            {
                return Unauthorized(new ErrorResponseDTO
                {
                    Message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                token
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO request)
        {
            await _authService.ForgotPasswordAsync(request);

            return Ok(new
            {
                message = "If the email exists, a temporary password has been sent."
            });
        }
    }
}
