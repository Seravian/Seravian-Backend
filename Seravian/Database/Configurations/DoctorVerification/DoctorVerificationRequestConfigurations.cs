using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class DoctorVerificationRequestConfigurations : IEntityTypeConfiguration<DoctorVerificationRequest>
{
    public void Configure(EntityTypeBuilder<DoctorVerificationRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasConversion<int>().IsRequired();
        builder.Property(e => e.Status).HasConversion<int>().IsRequired();
        builder.Property(e => e.RequestedAtUtc).IsRequired();

        builder.Property(e => e.RejectionNotes).HasMaxLength(2000).IsUnicode(true);

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
