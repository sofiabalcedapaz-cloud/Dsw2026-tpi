using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dsw2026Tpi.Data;

public class Dsw2026TpiDbContext : DbContext
{
    public Dsw2026TpiDbContext(DbContextOptions<Dsw2026TpiDbContext> options) : base(options)
    {
    }

    public DbSet<Speciality> Specialities { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}