using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleMaintenanceConfigurationController : ControllerBase
    {
        private readonly IVehicleMaintenanceConfigurationService _service;

        public VehicleMaintenanceConfigurationController(IVehicleMaintenanceConfigurationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<VehicleMaintenanceConfigurationDTO>> Create(VehicleMaintenanceConfigurationDTO dto)
        {
            var result = await _service.AddVehicleMaintenanceConfigurationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleMaintenanceConfigurationDTO>> GetById(int id)
        {
            var result = await _service.GetVehicleMaintenanceConfigurationByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleMaintenanceConfigurationDTO>>> GetAll()
        {
            var results = await _service.GetAllVehicleMaintenanceConfigurationsAsync();
            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VehicleMaintenanceConfigurationDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            Console.WriteLine($"ID: {dto.Id}");
            Console.WriteLine($"VehicleId: {dto.VehicleId}");
            Console.WriteLine($"VehicleRegistrationNo: {dto.VehicleRegistrationNo}");
            Console.WriteLine($"VehicleMaintenanceTypeId: {dto.VehicleMaintenanceTypeId}");
            Console.WriteLine($"TypeName: {dto.TypeName}");
            Console.WriteLine($"Duration: {dto.Duration}");
            Console.WriteLine($"Status: {dto.Status}");

            var result = await _service.UpdateVehicleMaintenanceConfigurationAsync(dto);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteVehicleMaintenanceConfigurationAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}