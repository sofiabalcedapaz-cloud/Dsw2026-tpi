using System;

namespace Dsw2026Tpi.Domain.Entities;

public class Speciality : EntityBase
{
    public string Name { get; init; }
    public string Description { get; init; }
    public bool Deleted { get; private set; }

#pragma warning disable CS8618
    private Speciality() { }
#pragma warning restore CS8618

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        Name = name;
        Description = description;
        Deleted = false;
    }

    public void Delete() => Deleted = true;
}