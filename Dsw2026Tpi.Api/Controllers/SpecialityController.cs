using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/specialties")]
public class SpecialtyController : AppController
{
    private readonly ISpecialtyService _service;

    public SpecialtyController(ISpecialtyService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageSize,
        [FromQuery] int pageIndex,
        [FromQuery] string? name = null)
    {
        var specialties = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialties);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SpecialtyRequest request)
    {
        var specialty = await _service.Create(request);
        return Created(string.Empty, specialty);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialtyRequest request)
    {
        var specialty = await _service.Update(id, request);
        return Ok(specialty);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("Ok");
    }
}