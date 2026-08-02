using System;

namespace Dsw2026Tpi.Domain.Entities;

public enum AppointmentStatus
{
    BOOKED,
    CANCELLED,
    ATTENDED,
    NO_SHOW
}

public class Appointment : EntityBase
{
    public Guid PatientId { get; private set; }
    public virtual Patient? Patient { get; private set; }
    public Guid AvailabilitySlotId { get; private set; }
    public virtual AvailabilitySlot? AvailabilitySlot { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string Reason { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool Deleted { get; private set; }

    private Appointment() { }

    public Appointment(Guid patientId, Guid availabilitySlotId, string reason, Guid? id = null) : base(id)
    {
        PatientId = patientId;
        AvailabilitySlotId = availabilitySlotId;
        Reason = reason;
        Status = AppointmentStatus.BOOKED;
        Deleted = false;
    }

    public void Cancel(string? reason = null)
    {
        Status = AppointmentStatus.CANCELLED;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void MarkAttended() => Status = AppointmentStatus.ATTENDED;
    public void MarkNoShow() => Status = AppointmentStatus.NO_SHOW;
    public void Delete() => Deleted = true;
}