using Dsw2026Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityRuleService
    {
        Task<IEnumerable<AvailabilityRuleModel.Response>> GetByDoctor(Guid doctorId);

        Task<IEnumerable<AvailabilityRuleModel.Response>> Create(AvailabilityRuleModel.Request request);
        Task<IEnumerable<AvailabilityRuleModel.Response>> Update(Guid doctorId, AvailabilityRuleModel.Request request);
    }
}
