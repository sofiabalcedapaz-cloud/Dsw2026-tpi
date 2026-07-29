namespace Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.CrossCutting.Identity;

public class AvailabilitySlot : EntityBase
{
    public Guid AvailabilityRuleId { get; private set; }
    public AvailabilityRule? AvailabilityRule { get; private set; }
    public DateOnly SlotDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public string Status { get; private set; }

#pragma warning disable CS8618
    private AvailabilitySlot() { }
#pragma warning restore CS8618

    public AvailabilitySlot(Guid availabilityRuleId, DateOnly slotDate, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
    {
        AvailabilityRuleId = availabilityRuleId;
        SlotDate = slotDate;
        StartTime = startTime;
        EndTime = endTime;
        Status = AvailabilitySlotStatus.Available;
    }
}
