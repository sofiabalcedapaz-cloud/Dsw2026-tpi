using System;

namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilitySlot : EntityBase
{
    public Guid DoctorId { get; private set; }
    public virtual Doctor Doctor { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsBooked { get; private set; }
    public bool Deleted { get; private set; }

    public virtual Appointment? Appointment { get; private set; }

#pragma warning disable CS8618
    private AvailabilitySlot() { }
#pragma warning restore CS8618

    public AvailabilitySlot(Guid doctorId, DateTime startTime, DateTime endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        StartTime = startTime;
        EndTime = endTime;
        IsBooked = false;
        Deleted = false;
    }

    public void Book() => IsBooked = true;
    public void Release() => IsBooked = false;
    public void Delete() => Deleted = true;
}