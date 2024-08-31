using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripDTO>>> GetAllTrips()
        {
            try
            {
                var trips = await _tripService.GetAllTripsAsync();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("dailycount")]
        public async Task<ActionResult<int>> GetDailyTripCount()
        {
            var count = await _tripService.GetDailyTripCountAsync();
            return Ok(count);
        }

        [HttpGet("{tripId}")]
        public async Task<ActionResult<Trip>> GetTripById(int tripId)
        {
            var trip = await _tripService.GetTripByIdAsync(tripId);
            if (trip == null)
                return NotFound();

            return Ok(trip);
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddTrip([FromBody] TripDTO tripDto)
        {
            try
            {
                var trip = await _tripService.AddTripAsync(tripDto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] TripDTO tripDTO)
        {
            try
            {
                var existingTrip = await _tripService.IsTripExist(id);

                if (!existingTrip)
                {
                    return NotFound("Trip with Id not found");
                }

                var trip = new Trip
                {
                    TripId = tripDTO.TripId,
                    Date = tripDTO.Date,
                    StartTime = tripDTO.StartTime,
                    EndTime = tripDTO.EndTime,
                    
                    StartMeterValue = tripDTO.StartMeterValue,
                    EndMeterValue = tripDTO.EndMeterValue,
                    Status = tripDTO.Status
                };

                var result = await _tripService.UpdateTripAsync(trip);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the trip: {ex.Message}");
            }
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateTrip(int id)
        {
            var result = await _tripService.ActivateTripAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateTrip(int id)
        {
            var result = await _tripService.DeactivateTripAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
      
    }
}
