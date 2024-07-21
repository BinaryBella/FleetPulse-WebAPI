﻿using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccidentsController : ControllerBase
    {
        private readonly IAccidentService _accidentService;

        public AccidentsController(IAccidentService accidentService)
        {
            _accidentService = accidentService;
        }
        
        [HttpGet("latest-month/count")]
        public ActionResult<int> GetLatestMonthAccidentCount()
        {
            var count = _accidentService.GetLatestMonthAccidentCount();
            return Ok(count);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccidentDTO>>> GetAllAccidents()
        {
            var accidents = await _accidentService.GetAllAccidentsAsync();
            return Ok(accidents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccidentDTO>> GetAccidentById(int id)
        {
            var accident = await _accidentService.GetAccidentByIdAsync(id);
            if (accident == null) return NotFound();
            return Ok(accident);
        }

        [HttpPost]
        public async Task<ActionResult<AccidentDTO>> CreateAccident([FromForm] AccidentCreateDTO accidentCreateDto)
        {
            var accident = await _accidentService.CreateAccidentAsync(accidentCreateDto);
            return CreatedAtAction(nameof(GetAccidentById), new { id = accident.AccidentId }, accident);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AccidentDTO>> UpdateAccident(int id, AccidentDTO accidentDto)
        {
            var updatedAccident = await _accidentService.UpdateAccidentAsync(id, accidentDto);
            if (updatedAccident == null) return NotFound();
            return Ok(updatedAccident);
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
