using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dsw2026Tpi.Application.Interfaces;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("availabilities")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public class AvailabilityController : AppController
    {
        private readonly IAvailabilityService _service;

        public AvailabilityController(IAvailabilityService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AvailabilityModel.Request request)
        {
            var availability = await _service.Create(request);
            return Created(string.Empty, availability);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AvailabilityModel.Request request)
        {
            var availability = await _service.Update(id, request);
            return Ok(availability);
        }
    }
}
