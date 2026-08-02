using System;
using System.Collections.Generic;

namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityRequest
{
    public Guid DoctorId { get; init; }
    public List<DayAvailabilityDto> Days { get; init; } = new();
}

public record DayAvailabilityDto
{
    public string Day { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
}

public record DoctorAvailabilityResponse
{
    public Guid Id { get; init; }
    public string Day { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
}

public record AvailabilitySlotResponse
{
    public Guid Id { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsBooked { get; init; }
}