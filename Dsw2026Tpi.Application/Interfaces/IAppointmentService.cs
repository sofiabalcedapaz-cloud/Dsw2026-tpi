using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponse> Book(AppointmentRequest request);
    Task<Pagination<AppointmentResponse>> GetPatientAppointments(long dni, int pageSize, int pageIndex);
    Task Cancel(Guid appointmentId);
    Task<Pagination<AppointmentResponse>> GetTodaysAppointments(DateTime date, int pageSize, int pageIndex);
    Task<Pagination<AppointmentSearchResponse>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateTime? date, int pageSize, int pageIndex);
}