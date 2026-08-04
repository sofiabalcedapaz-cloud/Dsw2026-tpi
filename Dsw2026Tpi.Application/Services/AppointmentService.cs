using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;

        public AppointmentService(IPersistence persistence)
        {
            _persistence = persistence;
        }
        public async Task<AppointmentModel.Response> Create(AppointmentModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 5)
            {
                throw new ValidationException(
                    "El motivo debe tener al menos 5 caracteres.",
                    nameof(ErrorCodes.VALIDATION_ERROR));
            }
            var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni)
                ?? throw new EntityNotFoundException(nameof(Patient));

            var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId, "AvailabilityRule.Doctor")
                ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            if (slot.AvailabilityRule!.DoctorId != request.DoctorId)
                throw new ValidationException("El turno no pertenece al médico indicado.", nameof(ErrorCodes.VALIDATION_ERROR));

            if (slot.Status != AvailabilitySlotStatus.Available)
                throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya fue reservado.");

            var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
            if (slotDateTime <= DateTime.UtcNow)
                throw new ValidationException("No se pueden reservar turnos en el pasado.", nameof(ErrorCodes.VALIDATION_ERROR));

            slot.Book();
            var appointment = new Appointment(slot.Id, patient.Id, request.Reason);

            try
            {
                await _persistence.Update(slot);
                await _persistence.Add(appointment);
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya fue reservado.")
                    .WithDetail("availabilitySlotId", "slot_unavailable");
            }
            return new AppointmentModel.Response(
                    appointment.Id,
                    appointment.Reason,
                    appointment.Status,
                    new AppointmentModel.SlotDto(
                        slot.Id,
                        slot.SlotDate,
                        slot.StartTime.ToString("HH:mm"),
                        slot.EndTime.ToString("HH:mm"),  
                        new AppointmentModel.DoctorDto(slot.AvailabilityRule.DoctorId, slot.AvailabilityRule.Doctor!.Name)),
                    new AppointmentModel.PatientDto(patient.Id, patient.Dni, patient.FullName));
        }

        public async Task Cancel(Guid id)
        {
            var appointment = await _persistence.GetById<Appointment>(id, nameof(Appointment.AvailabilitySlot));

            if (appointment is null)
            {
                throw new EntityNotFoundException(
                    nameof(Appointment));
            }

            if (appointment.Status != AppointmentStatus.Booked)
            {
                throw new ConflictException(
                    "APPOINTMENT_INVALID_STATUS",
                    "Solo se puede cancelar una cita en estado BOOKED.");
            }

            if (appointment.AvailabilitySlot is null)
            {
                throw new EntityNotFoundException(
                    nameof(AvailabilitySlot));
            }

            appointment.Cancel();
            appointment.AvailabilitySlot.Release();

            await _persistence.Update(
                appointment.AvailabilitySlot);

            await _persistence.Update(appointment);
        }

        public async Task<IEnumerable<AppointmentListModel.Response>> GetByPatient(long dni)
        {
            var appointments = await _persistence.GetFiltered<Appointment>(
                a => a.Patient!.Dni == dni && a.Status == AppointmentStatus.Booked,
                "AvailabilitySlot.AvailabilityRule.Doctor.Speciality", "Patient");

            return appointments?
                .OrderByDescending(a => a.AvailabilitySlot!.SlotDate)
                .Select(a => new AppointmentListModel.Response(
                    a.Id,
                    a.Reason,
                    a.Status,
                    a.AvailabilitySlot!.SlotDate,
                    a.AvailabilitySlot.StartTime.ToString("HH:mm"),
                    a.AvailabilitySlot.EndTime.ToString("HH:mm"),
                    a.AvailabilitySlot.AvailabilityRule!.Doctor!.Name,
                    a.AvailabilitySlot.AvailabilityRule.Doctor.Speciality!.Name))
                ?? [];
        }

        public async Task<Pagination<AppointmentListModel.Response>> GetByDate(DateOnly date, int pageSize, int pageIndex)
        {
            var appointments =
                await _persistence.Paginate<Appointment, TimeOnly>(
                    pageSize,
                    pageIndex,
                    a => a.AvailabilitySlot != null &&
                         a.AvailabilitySlot.SlotDate == date,
                    a => a.AvailabilitySlot!.StartTime,
                    "AvailabilitySlot.AvailabilityRule.Doctor.Speciality",
                    "Patient");

            return appointments.Map(a =>
                new AppointmentListModel.Response(
                    a.Id,
                    a.Reason,
                    a.Status,
                    a.AvailabilitySlot!.SlotDate,
                    a.AvailabilitySlot.StartTime.ToString("HH:mm"),
                    a.AvailabilitySlot.EndTime.ToString("HH:mm"),
                    a.AvailabilitySlot.AvailabilityRule!.Doctor!.Name,
                    a.AvailabilitySlot.AvailabilityRule.Doctor.Speciality!.Name));
        }

        public async Task<Pagination<AppointmentSearchModel.Item>> Search(AppointmentSearchModel.Request request)
        {
            var result = await _persistence.Paginate<Appointment, DateOnly>(
                request.PageSize,
                request.PageIndex,
                a =>
                    (!request.SpecialityId.HasValue || a.AvailabilitySlot!.AvailabilityRule!.Doctor!.SpecialityId == request.SpecialityId.Value) &&
                    (!request.DoctorId.HasValue || a.AvailabilitySlot!.AvailabilityRule!.DoctorId == request.DoctorId.Value) &&
                    (!request.Dni.HasValue || a.Patient!.Dni == request.Dni.Value) &&
                    (!request.Date.HasValue || a.AvailabilitySlot!.SlotDate == request.Date.Value),
                a => a.AvailabilitySlot!.SlotDate,
                "AvailabilitySlot.AvailabilityRule.Doctor.Speciality", "Patient");

            return result.Map(a => new AppointmentSearchModel.Item(
                a.Id,
                a.AvailabilitySlot!.AvailabilityRule!.Doctor!.Speciality!.Name,
                a.AvailabilitySlot.AvailabilityRule.Doctor.Name,
                a.AvailabilitySlot.SlotDate,
                a.AvailabilitySlot.StartTime.ToString("HH:mm"),
                a.Patient!.FullName,
                a.Status));
        }
    }

}
