using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/availabilities")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public class AvailabilityController : AppController
    {
        private readonly IAvailabilityRuleService _service;

        public AvailabilityController(IAvailabilityRuleService service)
        {
            _service = service;
        }

        [HttpGet("{doctorId:guid}")]
        [EnableRateLimiting(RateLimitPolices.General)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDoctor(Guid doctorId)
        {
            var availability = await _service.GetByDoctor(doctorId);
            return Ok(availability);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [EnableRateLimiting(RateLimitPolices.General)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AvailabilityRuleModel.Request request)
        {
            var availability = await _service.Create(request);
            return Created(string.Empty, availability);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting(RateLimitPolices.General)]
        public async Task<IActionResult> Update([FromBody] AvailabilityRuleModel.Request request)
        {
            var availability = await _service.Update(request.DoctorId, request);
            return Ok(availability);
        }
    }
}
