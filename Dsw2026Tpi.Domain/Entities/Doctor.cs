using System;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; init; }
    public string LicenseNumber { get; init; }
    public bool IsActive { get; private set; }
    public bool Deleted { get; private set; }
    public Guid? SpecialityId { get; set; }
    public virtual Speciality? Speciality { get; private set; }

    private readonly List<AvailabilitySlot> _availabilitySlots = new();

#pragma warning disable CS8618
    private Doctor() { }
#pragma warning restore CS8618

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

    public void AddAvailabilitySlot(AvailabilitySlot slot)
    {
        _availabilitySlots.Add(slot);
    }
}