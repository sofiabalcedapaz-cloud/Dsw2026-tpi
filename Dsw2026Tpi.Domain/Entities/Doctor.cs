using System;
using System.Collections.Generic;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public bool IsActive { get; private set; }
    public bool Deleted { get; private set; }
    public Guid? SpecialityId { get; private set; }
    public virtual Speciality? Speciality { get; private set; }

    private readonly List<AvailabilitySlot> _availabilitySlots = new();
    public IReadOnlyCollection<AvailabilitySlot> AvailabilitySlots => _availabilitySlots.AsReadOnly();

    private Doctor() { }

    public Doctor(string name, string licenseNumber, Speciality speciality, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality.Id;
        IsActive = true;
        Deleted = false;
    }

    public void Deactivate() => IsActive = false;
    public void Delete() => Deleted = true;
}