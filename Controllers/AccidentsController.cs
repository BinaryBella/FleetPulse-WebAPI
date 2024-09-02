using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccidentsController : ControllerBase
    {
        private readonly IAccidentService _accidentService;
        private readonly ILogger<AuthController> _logger;

        public AccidentsController(IAccidentService accidentService, ILogger<AuthController> logger)
        {
            _accidentService = accidentService;
            _logger = logger;
        }
        
        [HttpGet("latest-month/count")]
        public ActionResult<int> GetLatestMonthAccidentCount()
        {
            var count = _accidentService.GetLatestMonthAccidentCount();
            return Ok(count);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccidentDTO>>> GetAccidents()
        {
            var accidents = await _accidentService.GetAllAccidentsAsync();
            return Ok(accidents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccidentDTO>> GetAccidentById(int id)
        {
            var accident = await _accidentService.GetAccidentByIdAsync(id);
            if (accident == null)
                return NotFound();
            return Ok(accident);
        }
        
        
        [HttpGet("photos/{id}")]
        public async Task<ActionResult<List<byte[]>>> GetAccidentPhotos(int id)
        {
            var accidentPhotos = await _accidentService.GetAccidentPhotosAsync(id);
            var apiResponse = new ApiResponse
            {
                Status = true,
                Message = "Accident photos retrieved successfully",
                Data = accidentPhotos
            };
            if (accidentPhotos == null || accidentPhotos.Count == 0)
            {
                apiResponse.Data = new List<byte[]>();
                apiResponse.Message = "No photos found";
            }
            return new JsonResult(apiResponse);
        }
        
        
        [HttpPost]
        public async Task<ActionResult> CreateAccident([FromForm] AccidentCreateDTO accidentCreateDto)
        {
            _logger.LogInformation($"Received DTO: {JsonConvert.SerializeObject(accidentCreateDto)}");

            if (string.IsNullOrEmpty(accidentCreateDto.Venue))
            {
                return BadRequest("Venue is required.");
            }

            try
            {
                var accident = await _accidentService.CreateAccidentAsync(accidentCreateDto);
                var response = new ApiResponse
                {
                    Status = true,
                    Message = "Accident created successfully",
                };
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accident");
                return StatusCode(500, "An error occurred while creating the accident.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AccidentDTO>> UpdateAccident(int id, AccidentDTO accidentDto)
        {
            try
            {
                var updatedAccident = await _accidentService.UpdateAccidentAsync(id, accidentDto);
                var response = new ApiResponse
                {
                    Status = updatedAccident != null,
                    Message = "Accident created successfully"
                };
                if (updatedAccident == null) response.Message = "Accident Cannot Be Found";
                return Ok(updatedAccident);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<ActionResult> DeactivateAccident(int id)
        {
            var success = await _accidentService.DeactivateAccidentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/activate")]
        public async Task<ActionResult> ActivateAccident(int id)
        {
            var success = await _accidentService.ActivateAccidentAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
