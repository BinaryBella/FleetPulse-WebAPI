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
    public class HelperController : ControllerBase
    {
        private readonly IHelperService _helperService;

        public HelperController(IHelperService helperService)
        {
            _helperService = helperService ?? throw new ArgumentNullException(nameof(helperService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HelperDTO>>> GetAllHelpers()
        {
            var helpers = await _helperService.GetAllHelpersAsync();
            return Ok(helpers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HelperDTO>> GetHelperById(int id)
        {
            var helper = await _helperService.GetHelperByIdAsync(id);
            if (helper == null)
                return NotFound();

            return Ok(helper);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHelper(HelperDTO helperDto)
        {
            try
            {
                var createdHelper = await _helperService.CreateHelperAsync(helperDto);
                return CreatedAtAction(nameof(GetHelperById), new { id = createdHelper.UserId }, createdHelper);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHelper(int id, HelperDTO helperDto)
        {
            var updatedHelper = await _helperService.UpdateHelperAsync(id, helperDto);
            if (updatedHelper == null)
                return NotFound();

            return Ok(updatedHelper);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateHelper(int id)
        {
            var success = await _helperService.DeactivateHelperAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateHelper(int id)
        {
            var success = await _helperService.ActivateHelperAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var exists = await _helperService.DoesEmailExistAsync(email);
            return Ok(exists);
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            var exists = await _helperService.DoesUsernameExistAsync(username);
            return Ok(exists);
        }
    }
}
