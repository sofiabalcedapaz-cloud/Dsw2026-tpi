using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Tags("4. Disponibilidades")]
[Route("api/availabilities")]
[Authorize]
public class AvailabilityController : AppController
{
   private readonly IAvailabilityRuleService _service;

   public AvailabilityController(IAvailabilityRuleService service)
   {
         _service = service;
   }

   [HttpPost]
   [Authorize(Policy = Policies.AdminPolicy)]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [EnableRateLimiting(RateLimitPolices.General)]
   [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AvailabilityRuleModel.Request request)
    {
        var availability = await _service.Create(request);
        return Created(string.Empty, availability);
    }

    [HttpPut]
    [Authorize(Policy = Policies.AdminPolicy)]
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

