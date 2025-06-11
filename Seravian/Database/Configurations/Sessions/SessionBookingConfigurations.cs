using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionBookingConfiguration : IEntityTypeConfiguration<SessionBooking>
{
    public void Configure(EntityTypeBuilder<SessionBooking> builder)
    {
        builder.HasKey(sb => sb.Id);

        builder.Property(sb => sb.Status).IsRequired();

        builder.Property(sb => sb.DoctorNote).HasMaxLength(1000);

        builder.Property(sb => sb.PatientNote).HasMaxLength(1000);
        // Properties for media-related details
        builder
            .Property(sb => sb.Status)
            .HasConversion<int>() // Store enum as string
            .IsRequired();

        // 🔐 Concurrency token
        builder.Property(sb => sb.RowVersion).IsRowVersion().IsRequired();

        // Configure DateTime properties with UTC conversion
        builder
            .Property(sb => sb.CreatedAtUtc)
            .IsRequired()
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .Property(sb => sb.PatientIsAvailableFromUtc)
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .Property(sb => sb.PatientIsAvailableToUtc)
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .Property(sb => sb.ScheduledAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        builder
            .Property(sb => sb.UpdatedAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        builder.Property(sb => sb.PatientIsAvailableFromUtc).IsRequired();

        builder.Property(sb => sb.PatientIsAvailableToUtc).IsRequired();

        builder
            .HasOne(sb => sb.Doctor)
            .WithMany(d => d.SessionBookings)
            .HasForeignKey(sb => sb.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(sb => sb.Patient)
            .WithMany(p => p.SessionBookings)
            .HasForeignKey(sb => sb.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
