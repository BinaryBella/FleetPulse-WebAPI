using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService ?? throw new ArgumentNullException(nameof(staffService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDTO>>> GetAllStaff()
        {
            var staff = await _staffService.GetAllStaffAsync();
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StaffDTO>> GetStaffById(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
                return NotFound();

            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff(StaffDTO staffDto)
        {
            try
            {
                var createdStaff = await _staffService.CreateStaffAsync(staffDto);
                return new JsonResult(new ApiResponse
                {
                    Status = true,
                    Message = "Driver created successfully",
                    Data = createdStaff
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new JsonResult(new ApiResponse
                    {
                        Status = false,
                        Error = ex.Message
                    });
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, StaffDTO staffDto)
        {
            var updatedStaff = await _staffService.UpdateStaffAsync(id, staffDto);
            if (updatedStaff == null)
                return NotFound();

            return Ok(updatedStaff);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateStaff(int id)
        {
            var success = await _staffService.DeactivateStaffAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateStaff(int id)
        {
            var success = await _staffService.ActivateStaffAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var exists = await _staffService.DoesEmailExistAsync(email);
            return Ok(exists);
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            var exists = await _staffService.DoesUsernameExistAsync(username);
            return Ok(exists);
        }
    }
}
