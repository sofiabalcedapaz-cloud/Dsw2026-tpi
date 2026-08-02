using System;

namespace Dsw2026Tpi.Domain.Entities;

public class Speciality : EntityBase
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool Deleted { get; private set; }

    private Speciality() { }

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        Name = name;
        Description = description;
        Deleted = false;
    }

    public void Delete() => Deleted = true;
}