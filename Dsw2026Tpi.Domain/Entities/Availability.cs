using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Availability: EntityBase
    {
        public Guid DoctorId { get; private set; }
        public Doctor? Doctor { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Availability()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Availability(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            DoctorId = doctorId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }

        public void Update(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        {
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}

