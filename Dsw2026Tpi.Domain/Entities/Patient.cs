using System;
using System.Collections.Generic;

namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public long Dni { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }
    public bool Deleted { get; private set; }

    private readonly List<Appointment> _appointments = new();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private Patient() { }

    public Patient(string name, string email, long dni, string? phone = null, Guid? id = null) : base(id)
    {
        Name = name;
        Email = email;
        Dni = dni;
        Phone = phone;
        IsActive = true;
        Deleted = false;
    }

    public void Deactivate() => IsActive = false;
    public void Delete() => Deleted = true;
}