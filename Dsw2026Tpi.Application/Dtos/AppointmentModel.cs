using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientRequestDto Patient, string Reason);
        public record PatientRequestDto(long Dni);

        public record Response(Guid Id, string Reason, string Status, SlotDto Slot, PatientDto Patient);

        public record SlotDto(Guid Id, DateOnly SlotDate, TimeOnly StartTime, TimeOnly EndTime, DoctorDto Doctor);

        public record DoctorDto( Guid Id, string Name);

        public record PatientDto(Guid Id, long Dni, string FullName);
    }
}
