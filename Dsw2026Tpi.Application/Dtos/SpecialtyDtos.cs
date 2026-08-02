using System;

namespace Dsw2026Tpi.Application.Dtos;

public record SpecialtyRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record SpecialtyResponse(Guid Id, string Name, string Description);