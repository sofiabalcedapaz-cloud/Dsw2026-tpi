using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityRuleService
    {
        private readonly IPersistence _persistence;
        public AvailabilityRuleService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<IEnumerable<AvailabilityRuleModel.Response>> GetByDoctor(Guid doctorId)
        {
            var doctor = await _persistence.GetById<Doctor>(doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));
            var now = DateTime.UtcNow;
            var rules = await _persistence.GetFiltered<AvailabilityRule>(
                a => a.DoctorId == doctorId && a.Month == now.Month && a.Year == now.Year);

            return rules?.Select(a => new AvailabilityRuleModel.Response(
            a.Id,
            a.DoctorId,
            a.DayName,
            a.StartTimeFormatted,
            a.EndTimeFormatted)) ?? [];

        }
        public async Task<IEnumerable<AvailabilityRuleModel.Response>> Create(AvailabilityRuleModel.Request request)
        {
            _ = await _persistence.GetById<Doctor>(request.DoctorId)
                ?? throw new EntityNotFoundException(nameof(Doctor));

            var now = DateTime.UtcNow;
            var responses = new List<AvailabilityRuleModel.Response>();

            foreach (var day in request.Days)
            {
                if (!day.StartTime.IsValidTimeRange(day.EndTime))
                    throw new ValidationException("La hora de inicio debe ser anterior a la hora de fin.", nameof(ErrorCodes.VALIDATION_ERROR));

                await ValidateOverlap(request.DoctorId, day, now);

                var start = TimeOnly.Parse(day.StartTime);
                var end = TimeOnly.Parse(day.EndTime);
                var rule = new AvailabilityRule(request.DoctorId, (byte)now.Month, (short)now.Year, day.DayOfWeek, start, end);


                await _persistence.Add(rule);
                responses.Add(new AvailabilityRuleModel.Response(rule.Id, rule.DoctorId, rule.DayName, rule.StartTimeFormatted, rule.EndTimeFormatted));
            }
            return responses;
        }
        public async Task<IEnumerable<AvailabilityRuleModel.Response>> Update(Guid doctorId, AvailabilityRuleModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

            var current = await _persistence.GetFiltered<AvailabilityRule>(a => a.DoctorId == doctorId);
            if (current != null)
            {
                foreach (var old in current)
                    await _persistence.Delete(old);
            }
            return await Create(request);
        }

        private async Task ValidateOverlap(Guid doctorId, AvailabilityRuleModel.DayScheduleDto day, DateTime now)
        {
            var start = TimeOnly.Parse(day.StartTime);
            var end = TimeOnly.Parse(day.EndTime);

            var existing = await _persistence.First<AvailabilityRule>(a =>
                a.DoctorId == doctorId &&
                a.DayOfWeek == day.DayOfWeek &&
                a.Month == now.Month &&
                a.Year == now.Year &&
                ((start >= a.StartTime && start < a.EndTime) ||
                 (end > a.StartTime && end <= a.EndTime) ||
                 (start <= a.StartTime && end >= a.EndTime)));

            if (existing != null)
                throw new ConflictException("AVAILABILITY_OVERLAP", "El médico ya posee un horario que se solapa con el ingresado.");
        }

    }
}