using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AvailabilityService> _logger;
    private readonly List<DateTime> _holidays;

    public AvailabilityService(IPersistence persistence, ILogger<AvailabilityService> logger)
    {
        _persistence = persistence;
        _logger = logger;
        _holidays = LoadHolidays();
    }

    public async Task<List<DoctorAvailabilityResponse>> GetDoctorAvailabilities(Guid doctorId)
    {
        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException(nameof(Doctor));

        var slots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == doctorId && !s.Deleted && s.StartTime >= DateTime.UtcNow.Date,
            nameof(AvailabilitySlot.Doctor));

        if (!slots.Any())
            return new List<DoctorAvailabilityResponse>();

        var grouped = slots
            .Where(s => !s.IsBooked)
            .GroupBy(s => s.StartTime.DayOfWeek.ToString())
            .Select(g => new DoctorAvailabilityResponse
            {
                Id = Guid.NewGuid(),
                Day = g.Key,
                StartTime = g.Min(s => s.StartTime).ToString("HH:mm"),
                EndTime = g.Max(s => s.EndTime).ToString("HH:mm")
            })
            .ToList();

        return grouped;
    }

    public async Task<List<AvailabilitySlotResponse>> Create(AvailabilityRequest request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException(nameof(Doctor));

        var existingSlots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == request.DoctorId && !s.Deleted && s.StartTime.Month == DateTime.UtcNow.Month);

        if (existingSlots.Any())
            throw new ConflictException(nameof(ErrorCodes.DOCTOR_ALREADY_HAS_AVAILABILITY), "El médico ya tiene disponibilidad para este mes");

        var slots = GenerateSlotsForMonth(request.DoctorId, request.Days);
        var createdSlots = new List<AvailabilitySlot>();

        foreach (var slot in slots)
        {
            var created = await _persistence.Add(slot);
            createdSlots.Add(created);
        }

        _logger.LogInformation("Disponibilidad creada para médico {DoctorId}, {Count} slots generados", request.DoctorId, createdSlots.Count);

        return createdSlots.Select(s => new AvailabilitySlotResponse
        {
            Id = s.Id,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsBooked = s.IsBooked
        }).ToList();
    }

    public async Task<List<AvailabilitySlotResponse>> Update(AvailabilityRequest request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException(nameof(Doctor));

        var existingSlots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == request.DoctorId && !s.Deleted && s.StartTime.Month == DateTime.UtcNow.Month);

        foreach (var slot in existingSlots)
        {
            slot.Delete();
            await _persistence.Update(slot);
        }

        var slots = GenerateSlotsForMonth(request.DoctorId, request.Days);
        var createdSlots = new List<AvailabilitySlot>();

        foreach (var slot in slots)
        {
            var created = await _persistence.Add(slot);
            createdSlots.Add(created);
        }

        _logger.LogInformation("Disponibilidad actualizada para médico {DoctorId}, {Count} slots generados", request.DoctorId, createdSlots.Count);

        return createdSlots.Select(s => new AvailabilitySlotResponse
        {
            Id = s.Id,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsBooked = s.IsBooked
        }).ToList();
    }

    private List<AvailabilitySlot> GenerateSlotsForMonth(Guid doctorId, List<DayAvailabilityDto> days)
    {
        var slots = new List<AvailabilitySlot>();
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var dayMapping = new Dictionary<string, DayOfWeek>
        {
            { "LUNES", DayOfWeek.Monday },
            { "MARTES", DayOfWeek.Tuesday },
            { "MIERCOLES", DayOfWeek.Wednesday },
            { "JUEVES", DayOfWeek.Thursday },
            { "VIERNES", DayOfWeek.Friday },
            { "SABADO", DayOfWeek.Saturday },
            { "DOMINGO", DayOfWeek.Sunday }
        };

        foreach (var dayConfig in days)
        {
            if (!dayMapping.TryGetValue(dayConfig.Day.ToUpper(), out var dayOfWeek))
                throw new ValidationException($"Día inválido: {dayConfig.Day}", nameof(ErrorCodes.INVALID_DAY));

            if (!TimeOnly.TryParse(dayConfig.StartTime, out var startTime))
                throw new ValidationException($"Hora de inicio inválida: {dayConfig.StartTime}", nameof(ErrorCodes.TIME_RANGE_INVALID));

            if (!TimeOnly.TryParse(dayConfig.EndTime, out var endTime))
                throw new ValidationException($"Hora de fin inválida: {dayConfig.EndTime}", nameof(ErrorCodes.TIME_RANGE_INVALID));

            if (startTime >= endTime)
                throw new ValidationException("La hora de inicio debe ser menor a la hora de fin", nameof(ErrorCodes.TIME_RANGE_INVALID));

            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                if (date.DayOfWeek != dayOfWeek) continue;
                if (date < today) continue;
                if (_holidays.Any(h => h.Date == date.Date)) continue;

                var current = date.Date.Add(startTime.ToTimeSpan());
                var end = date.Date.Add(endTime.ToTimeSpan());

                while (current < end)
                {
                    var slotEnd = current.AddMinutes(30);
                    if (slotEnd > end) break;

                    slots.Add(new AvailabilitySlot(doctorId, current, slotEnd));
                    current = slotEnd;
                }
            }
        }

        return slots;
    }

    private void ValidateRequest(AvailabilityRequest request)
    {
        if (request.DoctorId == Guid.Empty)
            throw new ValidationException("DoctorId es requerido", nameof(ErrorCodes.VALIDATION_ERROR));

        if (request.Days == null || !request.Days.Any())
            throw new ValidationException("Debe especificar al menos un día", nameof(ErrorCodes.VALIDATION_ERROR));

        var dayGroups = request.Days.GroupBy(d => d.Day);
        foreach (var group in dayGroups)
        {
            if (group.Count() > 1)
                throw new ValidationException($"El día {group.Key} está duplicado", nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    private List<DateTime> LoadHolidays()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "holidays.json");
            if (!File.Exists(path))
                return new List<DateTime>();

            var json = File.ReadAllText(path);
            var holidays = JsonSerializer.Deserialize<List<HolidayDto>>(json);
            return holidays?.Select(h => h.Date).ToList() ?? new List<DateTime>();
        }
        catch
        {
            return new List<DateTime>();
        }
    }

    private class HolidayDto
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}