using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DoctorLanguageConfiguration : IEntityTypeConfiguration<DoctorLanguage>
{
    public void Configure(EntityTypeBuilder<DoctorLanguage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("bigint").ValueGeneratedOnAdd();

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10); // ISO 639-1 codes are 2-letter, but allow 10 for future safety

        builder
            .HasOne(x => x.DoctorVerificationRequest)
            .WithMany(x => x.Languages)
            .HasForeignKey(x => x.DoctorVerificationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional: prevent duplicate languages per request
        builder.HasIndex(x => new { x.DoctorVerificationRequestId, x.LanguageCode }).IsUnique();
    }
}
