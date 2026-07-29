using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public class AvailabilityModel
    {
        public record Request(Guid DoctorId, IEnumerable<DayScheduleDto> Days);

        public record DayScheduleDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

        public record Response(Guid Id, Guid DoctorId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
    }
}
