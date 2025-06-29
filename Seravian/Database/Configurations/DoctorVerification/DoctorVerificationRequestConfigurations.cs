using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class DoctorVerificationRequestConfigurations : IEntityTypeConfiguration<DoctorVerificationRequest>
{
    public void Configure(EntityTypeBuilder<DoctorVerificationRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(cm => cm.Id).HasColumnType("bigint").ValueGeneratedOnAdd();

        builder.Property(e => e.Title).HasConversion<int>().IsRequired();
        builder.Property(e => e.Status).HasConversion<int>().IsRequired();
        builder.Property(e => e.RequestedAtUtc).IsRequired();

        builder.Property(e => e.RejectionNote).HasMaxLength(2000).IsUnicode(true);

        builder
            .Property(d => d.Description)
            .HasMaxLength(1000) // or any max length you prefer
            .IsUnicode(true)
            .IsRequired();

        builder
            .Property(x => x.RequestedAtUtc)
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .Property(x => x.DeletedAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        builder
            .Property(x => x.ReviewedAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        // New fields
        builder.Property(x => x.TimeZone).IsRequired().HasMaxLength(100); // IANA TZ strings like "America/New_York"

        builder.Property(x => x.Nationality).IsRequired().HasMaxLength(2); // ISO 3166-1 alpha-2

        builder.Property(x => x.TimeZone).IsRequired().HasMaxLength(100); // IANA TZ strings like "America/New_York"

        // New relationships
        builder
            .HasMany(x => x.Languages)
            .WithOne(x => x.DoctorVerificationRequest)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.WorkingHours)
            .WithOne(x => x.DoctorVerificationRequest)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade); // New fields

        // New relationships
        builder
            .HasMany(x => x.Languages)
            .WithOne(x => x.DoctorVerificationRequest)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.WorkingHours)
            .WithOne(x => x.DoctorVerificationRequest)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(e => e.Attachments)
            .WithOne(a => a.DoctorVerificationRequest)
            .HasForeignKey(a => a.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // UNIQUE index on DoctorId WHERE Status = Pending
        builder.HasIndex(e => e.DoctorId).IsUnique().HasFilter("[Status] IN (0, 1)"); // assuming SQL

        // Configure RowVersion with Fluent API
        builder
            .Property(e => e.RowVersion) // Specify the property to configure
            .IsRowVersion() // Marks this property as a concurrency token
            .IsRequired(); // Optional: You can specify if the column is required
    }
}
