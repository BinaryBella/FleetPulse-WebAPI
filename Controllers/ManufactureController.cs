using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufactureController : ControllerBase
    {
        private readonly IManufactureService _manufactureService;

        public ManufactureController(IManufactureService manufactureService)
        {
            _manufactureService = manufactureService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Manufacture>>> GetAllManufactures()
        {
            var manufactures = await _manufactureService.GetAllManufacturesAsync();
            return Ok(manufactures);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Manufacture>> GetManufactureById(int id)
        {
            var manufacture = await _manufactureService.GetManufactureByIdAsync(id);
            if (manufacture == null)
                return NotFound();

            return Ok(manufacture);
        }

        [HttpPost]
        public async Task<ActionResult> AddManufactureAsync([FromBody] ManufactureDTO manufactureDto)
        {
            var response = new ApiResponse();
            try
            {
                var manufacture = new Manufacture
                {
                    Manufacturer = manufactureDto.Manufacturer,
                    Status = manufactureDto.Status
                };

                var manufactureExists = _manufactureService.DoesManufactureExists(manufacture.Manufacturer);
                if (manufactureExists)
                {
                    response.Message = "Manufacturer already exists";
                    return new JsonResult(response);
                }

                var addedManufacture = await _manufactureService.AddManufactureAsync(manufacture);

                if (addedManufacture != null)
                {
                    response.Status = true;
                    response.Message = "Added Successfully";
                    return new JsonResult(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to add Manufacturer";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }

            return new JsonResult(response);
        }
        
        [HttpPut("UpdateManufacture/{id}")]
        public async Task<IActionResult> UpdateManufacture(int id, [FromBody] ManufactureDTO manufactureDto)
        {
            try
            {
                // Check if the manufacturer exists by ID
                var existingManufacture = await _manufactureService.IsManufactureExist(id);

                if (!existingManufacture)
                {
                    return NotFound("Manufacturer with Id not found");
                }

                // Create a Manufacture entity from the DTO
                var manufacture = new Manufacture
                {
                    ManufactureId = id,  // Use the ID from the URL parameter
                    Manufacturer = manufactureDto.Manufacturer,
                    Status = manufactureDto.Status
                };

                // Update the manufacturer
                var updateResult = await _manufactureService.UpdateManufactureAsync(id, manufacture);

                if (updateResult)
                {
                    return Ok("Manufacturer updated successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to update manufacturer details.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, $"An error occurred while updating the manufacturer: {ex.Message}");
            }
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateManufacture(int id)
        {
            try
            {
                await _manufactureService.DeactivateManufactureAsync(id);
                return Ok("Manufacturer deactivated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateManufacture(int id)
        {
            try
            {
                await _manufactureService.ActivateManufactureAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
