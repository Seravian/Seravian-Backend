using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WorkingHoursTimeSlotConfiguration : IEntityTypeConfiguration<WorkingHoursTimeSlot>
{
    public void Configure(EntityTypeBuilder<WorkingHoursTimeSlot> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder
            .Property(x => x.DayOfWeek)
            .HasConversion<int>() // store enum as int
            .IsRequired();

        builder
            .Property(x => x.From)
            .HasConversion(v => v.ToTimeSpan(), v => TimeOnly.FromTimeSpan(v))
            .IsRequired();

        builder
            .Property(x => x.To)
            .HasConversion(v => v.ToTimeSpan(), v => TimeOnly.FromTimeSpan(v))
            .IsRequired();

        builder
            .HasOne(x => x.DoctorVerificationRequest)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional: prevent overlap at DB level by composite uniqueness
        builder.HasIndex(x => new
        {
            x.DoctorVerificationRequestId,
            x.DayOfWeek,
            x.From,
            x.To,
        });
    }
}
