using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Tags("2. Especialidades")]
[Route("api/specialities")]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;

    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting(RateLimitPolices.General)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageSize,
        [FromQuery] int pageIndex,
        [FromQuery] string? name = null)
    {
        var specialities = await _service.GetAll(pageSize, pageIndex, name);

        return Ok(specialities);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [EnableRateLimiting(RateLimitPolices.General)]
    public async Task<IActionResult> Create(
        [FromBody] SpecialityModel.Request request)
    {
        var speciality = await _service.Create(request);

        return Created(string.Empty, speciality);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting(RateLimitPolices.General)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] SpecialityModel.Request request)
    {
        var speciality = await _service.Update(id, request);

        return Ok(speciality);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting(RateLimitPolices.General)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);

        return Ok("Ok");
    }
}
