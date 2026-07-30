using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");

        builder.Property(a => a.Status)
            .HasMaxLength(20)
            .HasDefaultValue(AvailabilitySlotStatus.Available);

        builder.HasIndex(a => new { a.AvailabilityRuleId, a.SlotDate, a.StartTime }).IsUnique();
    }
}
