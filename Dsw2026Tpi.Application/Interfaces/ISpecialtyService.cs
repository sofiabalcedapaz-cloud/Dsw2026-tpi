using System;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface ISpecialtyService
{
    Task<Pagination<SpecialtyResponse>> GetAll(int pageSize, int pageIndex, string? name = null);
    Task<SpecialtyResponse?> GetById(Guid id);
    Task<SpecialtyResponse> Create(SpecialtyRequest request);
    Task<SpecialtyResponse> Update(Guid id, SpecialtyRequest request);
    Task Delete(Guid id);
}