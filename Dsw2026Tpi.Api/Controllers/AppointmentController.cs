using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/appointments")]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] AppointmentModel.Request request)
    {
        var appointment = await _service.Create(request);

        return Created(string.Empty, appointment);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _service.Cancel(id);

        return Ok("ok");
    }

    [HttpGet("patient")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(
        [FromQuery] long dni)
    {
        var appointments = await _service.GetByPatient(dni);

        return Ok(appointments);
    }

    [HttpGet]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateOnly date,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageIndex = 0)
    {
        var appointments = await _service.GetByDate(
            date,
            pageSize,
            pageIndex);

        return Ok(appointments);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? specialityId,
        [FromQuery] Guid? doctorId,
        [FromQuery] long? dni,
        [FromQuery] DateOnly? date,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageIndex = 0)
    {
        var request = new AppointmentSearchModel.Request(
            specialityId,
            doctorId,
            dni,
            date,
            pageSize,
            pageIndex);

        var appointments = await _service.Search(request);

        return Ok(appointments);
    }
}