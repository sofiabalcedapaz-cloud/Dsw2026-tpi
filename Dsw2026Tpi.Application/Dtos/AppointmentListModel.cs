using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentListModel
    {
        public record Response(Guid Id, string Reason, string Status, DateOnly SlotDate,string StartTime, string EndTime,string DoctorName,
                                string SpecialityName);
    }
}
