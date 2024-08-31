using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();

            return Ok(vehicle);
        }

        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetVehiclesCount()
        {
            try
            {
                var count = await _vehicleService.GetVehicleCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve Vehicles count: {ex.Message}");
            }
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleDTO vehicleDto)
        {
            if (!await _vehicleService.IsRegistrationNoUniqueAsync(vehicleDto.VehicleRegistrationNo))
                return BadRequest("Vehicle registration number must be unique.");

            var addedVehicle = await _vehicleService.AddVehicleAsync(vehicleDto);
            return CreatedAtAction(nameof(GetVehicleById), new { id = addedVehicle.VehicleId }, addedVehicle);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDTO vehicleDto)
        {
            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleDto);
            if (updatedVehicle == null) return NotFound();

            return Ok(updatedVehicle);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateVehicle(int id)
        {
            var result = await _vehicleService.ActivateVehicleAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateVehicle(int id)
        {
            var result = await _vehicleService.DeactivateVehicleAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
