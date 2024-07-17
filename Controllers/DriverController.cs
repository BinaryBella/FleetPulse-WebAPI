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
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DriverDTO>>> GetAllDrivers()
        {
            var drivers = await _driverService.GetAllDriversAsync();
            return Ok(drivers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DriverDTO>> GetDriverById(int id)
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
                return NotFound();

            return Ok(driver);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver(DriverDTO driverDto)
        {
            try
            {
                var createdDriver = await _driverService.CreateDriverAsync(driverDto);
                return CreatedAtAction(nameof(GetDriverById), new { id = createdDriver.UserId }, createdDriver);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, DriverDTO driverDto)
        {
            var updatedDriver = await _driverService.UpdateDriverAsync(id, driverDto);
            if (updatedDriver == null)
                return NotFound();

            return Ok(updatedDriver);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateDriver(int id)
        {
            var success = await _driverService.DeactivateDriverAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateDriver(int id)
        {
            var success = await _driverService.ActivateDriverAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var exists = await _driverService.DoesEmailExistAsync(email);
            return Ok(exists);
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            var exists = await _driverService.DoesUsernameExistAsync(username);
            return Ok(exists);
        }
    }
}
