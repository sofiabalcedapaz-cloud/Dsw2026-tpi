using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("APPOINTMENTS");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Reason)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(a => a.AvailabilitySlotId)
                .IsUnique()
                .HasFilter("[Status] = 'BOOKED'");

            builder.HasOne(a => a.AvailabilitySlot)
                .WithMany()
                .HasForeignKey(a => a.AvailabilitySlotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}