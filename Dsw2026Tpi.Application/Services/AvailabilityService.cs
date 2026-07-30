using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService
    {
        private readonly IPersistence _persistence;
        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<IEnumerable<AvailabilityModel.Response>> Create(AvailabilityModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor));
            var responses = new List<AvailabilityModel.Response>();

            foreach (var day in request.Days)
            {
                await ValidateDay(request.DoctorId, day);

                var availability = new Availability(
                         request.DoctorId,
                         day.DayOfWeek,
                         day.StartTime,
                         day.EndTime
                );

                await _persistence.Add(availability);

                responses.Add(new AvailabilityModel.Response(
                        availability.Id,
                        availability.DoctorId,
                        availability.DayOfWeek,
                        availability.StartTime,
                        availability.EndTime
                    ));
            }
            return responses;
        }
        public async Task<IEnumerable<AvailabilityModel.Response>> Update(Guid doctorId, AvailabilityModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

            var current = await _persistence.GetFiltered<Availability>(a => a.DoctorId == doctorId);
            if (current != null)
            {
                foreach (var old in current)
                    await _persistence.Delete(old);
            }
            return await Create(request);
        }

        private async Task ValidateDay(Guid doctorId, AvailabilityModel.DayScheduleDto day)
        {
            if (day.StartTime >= day.EndTime)
            {
                throw new ValidationException("La hora de inicio debe ser anterior a la hora de fin.",
                          nameof(ErrorCodes.VALIDATION_ERROR)
                );
            }
            var existing = await _persistence.First<Availability>(a =>
              a.DoctorId == doctorId &&
              a.DayOfWeek == day.DayOfWeek && ((day.StartTime >= a.StartTime && day.StartTime < a.EndTime) ||
                (day.EndTime > a.StartTime && day.EndTime <= a.EndTime) ||
                (day.StartTime <= a.StartTime && day.EndTime >= a.EndTime))
             );

            if (existing != null)
            {
                throw new ConflictException("AVAILABILITY_ALREADY_EXISTS", "El médico ya posee un horario registrado que se solapa con el ingresado.");
            }
        }

    }
}