using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AppointmentService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AppointmentService(IPersistence persistence, ILogger<AppointmentService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<AppointmentResponse> Book(AppointmentRequest request)
    {
        ValidateRequest(request);

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni && !p.Deleted);
        if (patient is null)
        {
            if (string.IsNullOrWhiteSpace(request.Patient.Name) || string.IsNullOrWhiteSpace(request.Patient.Email))
                throw new ValidationException("Para registrar un nuevo paciente se requiere Nombre y Email", nameof(ErrorCodes.VALIDATION_ERROR));

            patient = new Patient(
                request.Patient.Name,
                request.Patient.Email,
                request.Patient.Dni,
                request.Patient.Phone
            );
            await _persistence.Add(patient);
            _logger.LogInformation("Paciente registrado: {Name} (DNI: {Dni})", patient.Name, patient.Dni);
        }

        await _semaphore.WaitAsync();
        try
        {
            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId, nameof(AvailabilitySlot.Doctor));
            if (slot is null || slot.Deleted)
                throw new EntityNotFoundException(nameof(AvailabilitySlot));

            if (slot.IsBooked)
                throw new ConflictException(nameof(ErrorCodes.SLOT_ALREADY_BOOKED), "El turno ya fue reservado");

            if (slot.StartTime < DateTime.UtcNow)
                throw new ValidationException("No se pueden reservar turnos en el pasado", nameof(ErrorCodes.APPOINTMENT_IN_PAST));

            if (slot.DoctorId != request.DoctorId)
                throw new ValidationException("El slot no corresponde al médico seleccionado", nameof(ErrorCodes.VALIDATION_ERROR));

            var appointment = new Appointment(patient.Id, slot.Id, request.Reason);
            slot.Book();

            await _persistence.Update(slot);
            await _persistence.Add(appointment);

            _logger.LogInformation("Turno reservado: Paciente {PatientId}, Slot {SlotId}", patient.Id, slot.Id);

            return new AppointmentResponse
            {
                Id = appointment.Id,
                DoctorId = slot.DoctorId,
                DoctorName = slot.Doctor?.Name ?? string.Empty,
                SpecialtyName = slot.Doctor?.Speciality?.Name ?? string.Empty,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Status = appointment.Status.ToString(),
                Reason = appointment.Reason
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Pagination<AppointmentResponse>> GetPatientAppointments(long dni, int pageSize, int pageIndex)
    {
        var patient = await _persistence.First<Patient>(p => p.Dni == dni && !p.Deleted);
        if (patient is null)
            throw new EntityNotFoundException(nameof(Patient));

        var appointments = await _persistence.Paginate<Appointment, DateTime>(
            pageSize,
            pageIndex,
            a => a.PatientId == patient.Id && !a.Deleted && a.Status != AppointmentStatus.CANCELLED,
            a => a.CreatedAt,
            nameof(Appointment.AvailabilitySlot),
            nameof(Appointment.AvailabilitySlot.Doctor),
            nameof(Appointment.AvailabilitySlot.Doctor.Speciality));

        return appointments.Map(a => new AppointmentResponse
        {
            Id = a.Id,
            DoctorId = a.AvailabilitySlot.DoctorId,
            DoctorName = a.AvailabilitySlot.Doctor?.Name ?? string.Empty,
            SpecialtyName = a.AvailabilitySlot.Doctor?.Speciality?.Name ?? string.Empty,
            StartTime = a.AvailabilitySlot.StartTime,
            EndTime = a.AvailabilitySlot.EndTime,
            Status = a.Status.ToString(),
            Reason = a.Reason
        });
    }

    public async Task Cancel(Guid appointmentId)
    {
        var appointment = await _persistence.GetById<Appointment>(appointmentId, nameof(Appointment.AvailabilitySlot));
        if (appointment is null || appointment.Deleted)
            throw new EntityNotFoundException(nameof(Appointment));

        if (appointment.Status == AppointmentStatus.CANCELLED)
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_ALREADY_CANCELLED), "El turno ya fue cancelado");

        if (appointment.Status == AppointmentStatus.ATTENDED || appointment.Status == AppointmentStatus.NO_SHOW)
            throw new ConflictException("APPOINTMENT_CANNOT_CANCEL", "No se puede cancelar un turno atendido o no presentado");

        appointment.Cancel();
        appointment.AvailabilitySlot.Release();

        await _persistence.Update(appointment);
        await _persistence.Update(appointment.AvailabilitySlot);

        _logger.LogInformation("Turno cancelado: {AppointmentId}", appointmentId);
    }

    public async Task<Pagination<AppointmentResponse>> GetTodaysAppointments(DateTime date, int pageSize, int pageIndex)
    {
        var start = date.Date;
        var end = date.Date.AddDays(1);

        var appointments = await _persistence.Paginate<Appointment, DateTime>(
            pageSize,
            pageIndex,
            a => a.AvailabilitySlot.StartTime >= start &&
                  a.AvailabilitySlot.StartTime < end &&
                  !a.Deleted &&
                  a.Status != AppointmentStatus.CANCELLED,
            a => a.AvailabilitySlot.StartTime,
            nameof(Appointment.AvailabilitySlot),
            nameof(Appointment.AvailabilitySlot.Doctor),
            nameof(Appointment.AvailabilitySlot.Doctor.Speciality));

        return appointments.Map(a => new AppointmentResponse
        {
            Id = a.Id,
            DoctorId = a.AvailabilitySlot.DoctorId,
            DoctorName = a.AvailabilitySlot.Doctor?.Name ?? string.Empty,
            SpecialtyName = a.AvailabilitySlot.Doctor?.Speciality?.Name ?? string.Empty,
            StartTime = a.AvailabilitySlot.StartTime,
            EndTime = a.AvailabilitySlot.EndTime,
            Status = a.Status.ToString(),
            Reason = a.Reason
        });
    }

    public async Task<Pagination<AppointmentSearchResponse>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateTime? date, int pageSize, int pageIndex)
    {
        var query = await _persistence.GetFiltered<Appointment>(
            a => !a.Deleted && a.Status != AppointmentStatus.CANCELLED,
            nameof(Appointment.AvailabilitySlot),
            nameof(Appointment.AvailabilitySlot.Doctor),
            nameof(Appointment.AvailabilitySlot.Doctor.Speciality),
            nameof(Appointment.Patient));

        if (specialtyId.HasValue)
            query = query.Where(a => a.AvailabilitySlot.Doctor.SpecialityId == specialtyId.Value);

        if (doctorId.HasValue)
            query = query.Where(a => a.AvailabilitySlot.DoctorId == doctorId.Value);

        if (dni.HasValue)
            query = query.Where(a => a.Patient.Dni == dni.Value);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = date.Value.Date.AddDays(1);
            query = query.Where(a => a.AvailabilitySlot.StartTime >= start && a.AvailabilitySlot.StartTime < end);
        }

        var ordered = query.OrderBy(a => a.AvailabilitySlot.StartTime);
        var total = ordered.Count();

        var paged = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var data = paged.Select(a => new AppointmentSearchResponse
        {
            Specialty = a.AvailabilitySlot.Doctor.Speciality?.Name ?? string.Empty,
            Doctor = a.AvailabilitySlot.Doctor?.Name ?? string.Empty,
            AvailableTime = a.AvailabilitySlot.StartTime
        }).ToList();

        return new Pagination<AppointmentSearchResponse>(pageSize, pageIndex, total, data);
    }

    private void ValidateRequest(AppointmentRequest request)
    {
        if (request.DoctorId == Guid.Empty)
            throw new ValidationException("DoctorId es requerido", nameof(ErrorCodes.VALIDATION_ERROR));

        if (request.AvailabilitySlotId == Guid.Empty)
            throw new ValidationException("AvailabilitySlotId es requerido", nameof(ErrorCodes.VALIDATION_ERROR));

        var dniStr = request.Patient.Dni.ToString();
        if (dniStr.Length < 7 || dniStr.Length > 10)
            throw new ValidationException("El DNI debe tener entre 7 y 10 dígitos", nameof(ErrorCodes.VALIDATION_ERROR));

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
            throw new ValidationException("El motivo debe tener al menos 5 caracteres", nameof(ErrorCodes.VALIDATION_ERROR));
    }
}
