using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRules");

        builder.Property(a => a.Month).HasColumnType("tinyint");
        builder.Property(a => a.Year).HasColumnType("smallint");
        builder.Property(a => a.DayOfWeek).HasColumnType("tinyint");

        builder.HasIndex(a => new { a.DoctorId, a.Year, a.Month, a.DayOfWeek, a.StartTime, a.EndTime }).IsUnique();
    }
}
