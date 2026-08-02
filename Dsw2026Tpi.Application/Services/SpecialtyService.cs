using System;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<SpecialtyService> _logger;

    public SpecialtyService(IPersistence persistence, ILogger<SpecialtyService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<Pagination<SpecialtyResponse>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var specialties = await _persistence.Paginate<Speciality, string>(
            pageSize,
            pageIndex,
            s => (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)) && !s.Deleted,
            s => s.Name
        );

        return specialties.Map(s => new SpecialtyResponse(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialtyResponse?> GetById(Guid id)
    {
        var specialty = await _persistence.GetById<Speciality>(id);
        if (specialty is null || specialty.Deleted)
            return null;

        return new SpecialtyResponse(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task<SpecialtyResponse> Create(SpecialtyRequest request)
    {
        ValidateRequest(request);

        var existing = await _persistence.First<Speciality>(s => s.Name == request.Name && !s.Deleted);
        if (existing is not null)
            throw new ConflictException(nameof(ErrorCodes.SPECIALITY_DUPLICATED), "La especialidad ya existe");

        var specialty = new Speciality(request.Name, request.Description);
        await _persistence.Add(specialty);

        _logger.LogInformation("Especialidad creada: {Name} (Id: {Id})", specialty.Name, specialty.Id);

        return new SpecialtyResponse(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task<SpecialtyResponse> Update(Guid id, SpecialtyRequest request)
    {
        ValidateRequest(request);

        var specialty = await _persistence.GetById<Speciality>(id);
        if (specialty is null || specialty.Deleted)
            throw new EntityNotFoundException(nameof(Speciality));

        var existing = await _persistence.First<Speciality>(s => s.Name == request.Name && s.Id != id && !s.Deleted);
        if (existing is not null)
            throw new ConflictException(nameof(ErrorCodes.SPECIALITY_DUPLICATED), "La especialidad ya existe");

        var updatedSpecialty = new Speciality(request.Name, request.Description, id);
        await _persistence.Update(updatedSpecialty);

        _logger.LogInformation("Especialidad actualizada: {Name} (Id: {Id})", updatedSpecialty.Name, updatedSpecialty.Id);

        return new SpecialtyResponse(updatedSpecialty.Id, updatedSpecialty.Name, updatedSpecialty.Description);
    }

    public async Task Delete(Guid id)
    {
        var specialty = await _persistence.GetById<Speciality>(id);
        if (specialty is null || specialty.Deleted)
            throw new EntityNotFoundException(nameof(Speciality));

        specialty.Delete();
        await _persistence.Update(specialty);

        _logger.LogInformation("Especialidad eliminada (soft): {Name} (Id: {Id})", specialty.Name, specialty.Id);
    }

    private static void ValidateRequest(SpecialtyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres", nameof(ErrorCodes.VALIDATION_ERROR));

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
            throw new ValidationException("La descripción debe tener entre 10 y 100 caracteres", nameof(ErrorCodes.VALIDATION_ERROR));
    }
}