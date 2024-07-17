using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Controllers
{
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
                return CreatedAtAction(nameof(GetStaffById), new { id = createdStaff.UserId }, createdStaff);
            }
            catch (Exception ex)
            {
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

        [HttpDelete("{id}")]
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
