using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public class AvailabilityRuleModel
    {
        public record Request(Guid DoctorId, IEnumerable<DayScheduleDto> Days);

        public record DayScheduleDto(byte DayOfWeek, string StartTime, string EndTime);

        public record Response(Guid Id, Guid DoctorId, string Day, string StartTime, string EndTime);
    }
}
