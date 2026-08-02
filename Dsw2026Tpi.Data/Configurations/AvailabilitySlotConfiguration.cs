using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.IsBooked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Deleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.DoctorId, x.StartTime, x.EndTime })
            .IsUnique()
            .HasFilter("[Deleted] = 0");

        builder.HasIndex(x => x.IsBooked);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}