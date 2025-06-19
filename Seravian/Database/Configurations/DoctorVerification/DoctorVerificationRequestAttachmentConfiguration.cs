using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DoctorVerificationRequestAttachmentConfiguration
    : IEntityTypeConfiguration<DoctorVerificationRequestAttachment>
{
    public void Configure(EntityTypeBuilder<DoctorVerificationRequestAttachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);

        builder.Property(a => a.FilePath).IsRequired();

        builder.Property(a => a.ContentType).HasMaxLength(100);

        builder.Property(a => a.Description).HasMaxLength(500);

        builder.Property(a => a.UploadedAtUtc).IsRequired();

        builder.Property(a => a.FileSize).IsRequired();

        builder
            .Property(x => x.UploadedAtUtc)
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .HasOne(a => a.DoctorVerificationRequest)
            .WithMany(r => r.Attachments)
            .HasForeignKey(a => a.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
