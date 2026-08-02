using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<DoctorAvailabilityResponse>> GetDoctorAvailabilities(Guid doctorId);
    Task<List<AvailabilitySlotResponse>> Create(AvailabilityRequest request);
    Task<List<AvailabilitySlotResponse>> Update(AvailabilityRequest request);
}