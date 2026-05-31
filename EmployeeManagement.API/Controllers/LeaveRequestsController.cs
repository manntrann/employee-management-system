using EmployeeManagement.API.DTOs.Common;
using EmployeeManagement.API.DTOs.LeaveRequestDTO;
using EmployeeManagement.API.Services.Interfaces;
using EmployeeManagement.API.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EmployeeManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/leave-requests")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(LeaveRequestCreateDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var outcome = await _leaveRequestService.CreateForUserAsync(userId.Value, dto);

            return outcome.Result switch
            {
                LeaveRequestCreateResult.Created => Ok(outcome.LeaveRequest),
                LeaveRequestCreateResult.InvalidDateRange => BadRequest(new ErrorResponseDTO
                {
                    Message = "End date must be on or after start date."
                }),
                LeaveRequestCreateResult.NoEmployeeProfile => BadRequest(new ErrorResponseDTO
                {
                    Message = "No employee profile is linked to your account."
                }),
                LeaveRequestCreateResult.OverlappingLeave => Conflict(new ErrorResponseDTO
                {
                    Message = "Leave dates overlap with an existing pending or approved request."
                }),
                LeaveRequestCreateResult.InsufficientBalance => Conflict(new ErrorResponseDTO
                {
                    Message = "Insufficient leave balance for the requested dates."
                }),
                _ => BadRequest(new ErrorResponseDTO { Message = "Unable to create leave request." })
            };
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var items = await _leaveRequestService.GetMineAsync(userId.Value);
            return Ok(items);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetMyBalance()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var balance = await _leaveRequestService.GetBalanceForUserAsync(userId.Value);
            if (balance == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "No employee profile is linked to your account." });
            }

            return Ok(balance);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _leaveRequestService.GetAllAsync();
            return Ok(items);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var result = await _leaveRequestService.CancelMineAsync(userId.Value, id);

            return result switch
            {
                LeaveRequestCancelResult.Success => NoContent(),
                LeaveRequestCancelResult.NotFound => NotFound(new ErrorResponseDTO { Message = "Leave request not found." }),
                LeaveRequestCancelResult.NotOwner => Forbid(),
                LeaveRequestCancelResult.NotPending => BadRequest(new ErrorResponseDTO
                {
                    Message = "Only pending leave requests can be cancelled."
                }),
                _ => BadRequest(new ErrorResponseDTO { Message = "Unable to cancel leave request." })
            };
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, LeaveRequestReviewDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var result = await _leaveRequestService.ApproveAsync(userId.Value, id, dto.Note);

            return MapReviewResult(result, "approve");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, LeaveRequestReviewDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponseDTO { Message = "Invalid token." });
            }

            var result = await _leaveRequestService.RejectAsync(userId.Value, id, dto.Note);

            return MapReviewResult(result, "reject");
        }

        private IActionResult MapReviewResult(LeaveRequestReviewResult result, string action)
        {
            return result switch
            {
                LeaveRequestReviewResult.Success => NoContent(),
                LeaveRequestReviewResult.NotFound => NotFound(new ErrorResponseDTO { Message = "Leave request not found." }),
                LeaveRequestReviewResult.NotPending => BadRequest(new ErrorResponseDTO
                {
                    Message = "Only pending leave requests can be reviewed."
                }),
                LeaveRequestReviewResult.InsufficientBalance => Conflict(new ErrorResponseDTO
                {
                    Message = "Insufficient leave balance to approve this request."
                }),
                _ => BadRequest(new ErrorResponseDTO { Message = $"Unable to {action} leave request." })
            };
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
