using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentSearchModel
    {
        public record Request(Guid? SpecialityId, Guid? DoctorId, long? Dni, DateOnly? Date, int PageSize = 10, int PageIndex = 1);

        public record Item(Guid Id, string SpecialityName, string DoctorName, DateOnly Date, string StartTime, string PatientName,string Status);
    }
}
