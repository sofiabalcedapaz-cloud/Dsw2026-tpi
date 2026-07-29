namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilityRule : EntityBase
{
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }
    public byte Month { get; private set; }
    public short Year { get; private set; }
    public byte DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }



    #region Constructor for EF
#pragma warning disable CS8618
    private AvailabilityRule()
    {
    }
#pragma warning restore CS8618
    #endregion
    public AvailabilityRule(Guid doctorId, byte month, short year, byte dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        Month = month;
        Year = year;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }
}