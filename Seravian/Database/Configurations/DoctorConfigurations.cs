using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class DoctorConfigurations : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.UserId); // UserId cannot be null;
        builder
            .Property(d => d.Description)
            .HasMaxLength(1000) // or any max length you prefer
            .IsUnicode(true);
        builder.Property(d => d.Title).HasConversion<int?>();

        builder
            .Property(x => x.VerifiedAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        builder.HasOne(x => x.User).WithOne().HasForeignKey<Doctor>(d => d.UserId);

        builder
            .HasMany(d => d.DoctorVerificationRequests)
            .WithOne(vr => vr.Doctor)
            .HasForeignKey(vr => vr.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(d => d.SessionBookings)
            .WithOne(sb => sb.Doctor)
            .HasForeignKey(sb => sb.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
