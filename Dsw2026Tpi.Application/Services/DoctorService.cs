using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize,pageIndex, d => d.IsActive && (string.IsNullOrWhiteSpace(name) ||
                  d.Name.Contains(name)), d => d.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        await ValidateRequest(request);

        var speciality =await _persistence.GetById<Speciality>(request.SpecialityId);

        if (speciality is null)
        {
            throw new EntityNotFoundException(nameof(Speciality));
        }

        var doctor = new Doctor(request.Name,request.LicenseNumber, speciality);

        await _persistence.Add(doctor);
        return MapResponse(doctor);
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(id, nameof(Doctor.Speciality));

        if (doctor is null || !doctor.IsActive)
        {
            throw new EntityNotFoundException(nameof(Doctor));
        }

        await ValidateRequest(request, id);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);

        if (speciality is null)
        {
            throw new EntityNotFoundException(nameof(Speciality));
        }

        doctor.Update(request.Name, request.LicenseNumber, speciality);

        await _persistence.Update(doctor);

        return MapResponse(doctor);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || !doctor.IsActive)
        {
            throw new EntityNotFoundException(nameof(Doctor));
        }

        doctor.Deactivate();

        await _persistence.Update(doctor);
    }

    private async Task ValidateRequest(DoctorModel.Request request,Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
        {
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres.", nameof(ErrorCodes.VALIDATION_ERROR));
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            throw new ValidationException("La matrícula es obligatoria.", nameof(ErrorCodes.VALIDATION_ERROR));
        }

        if (request.SpecialityId == Guid.Empty)
        {
            throw new ValidationException( "La especialidad es obligatoria.", nameof(ErrorCodes.VALIDATION_ERROR));
        }

        var existing =await _persistence.First<Doctor>(d => d.LicenseNumber == request.LicenseNumber);
        if (existing is not null && existing.Id != id)
        {
            throw new ConflictException("DOCTOR_ALREADY_EXISTS","Ya existe un médico con esa matrícula.");
        }
    }

    private static DoctorModel.Response MapResponse(Doctor doctor)
    {
        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,new DoctorModel.SpecialityDto(doctor.Speciality?.Id, doctor.Speciality?.Name));
    }
}
