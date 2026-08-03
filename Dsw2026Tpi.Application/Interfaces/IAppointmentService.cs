using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> Create(AppointmentModel.Request request);
        Task<IEnumerable<AppointmentListModel.Response>> GetByPatient(long dni);
        Task<Pagination<AppointmentListModel.Response>> GetByDate(DateOnly date,int pageSize,int pageIndex);
        Task Cancel(Guid id);
        Task<Pagination<AppointmentSearchModel.Item>> Search(AppointmentSearchModel.Request request);
    }
}

