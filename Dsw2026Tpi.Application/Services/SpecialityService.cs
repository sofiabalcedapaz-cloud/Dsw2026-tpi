using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;

    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex,
            s => string.IsNullOrWhiteSpace(name) ||
                 s.Name.Contains(name),
            s => s.Name);

        return specialities.Map(s =>
            new SpecialityModel.Response(
                s.Id,
                s.Name,
                s.Description));
    }

    public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
    {
        await ValidateRequest(request);
        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        var speciality = await _persistence.GetById<Speciality>(id);
        if (speciality is null)
        {
            throw new EntityNotFoundException(nameof(Speciality));
        }

        await ValidateRequest(request, id);
        speciality.Update(request.Name, request.Description);
        await _persistence.Update(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id);

        if (speciality is null)
        {
            throw new EntityNotFoundException(nameof(Speciality));
        }
         
        await _persistence.Delete(speciality);
    }

    private async Task ValidateRequest(SpecialityModel.Request request, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
        {
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
        {
            throw new ValidationException("La descripción debe tener entre 10 y 100 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));
        }

        var existing = await _persistence.First<Speciality>( s => s.Name == request.Name);

        if (existing != null && existing.Id != id)
        {
            throw new ConflictException( "SPECIALITY_ALREADY_EXISTS", "Ya existe una especialidad con ese nombre.");
        }
    }
}
