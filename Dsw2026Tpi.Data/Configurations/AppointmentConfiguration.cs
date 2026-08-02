using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Deleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.AvailabilitySlotId)
            .IsUnique()
            .HasFilter("[Deleted] = 0");

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AvailabilitySlot)
            .WithOne(x => x.Appointment)
            .HasForeignKey<Appointment>(x => x.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}