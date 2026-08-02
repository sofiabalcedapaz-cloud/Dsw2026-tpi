using System;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentRequest
{
    public Guid DoctorId { get; init; }
    public Guid AvailabilitySlotId { get; init; }
    public PatientInfoDto Patient { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}

public record PatientInfoDto
{
    public long Dni { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

public record AppointmentResponse
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public string SpecialtyName { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record AppointmentSearchResponse
{
    public string Specialty { get; init; } = string.Empty;
    public string Doctor { get; init; } = string.Empty;
    public DateTime AvailableTime { get; init; }
}