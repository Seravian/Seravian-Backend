using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChatDiagnosisPrescriptionConfiguration
    : IEntityTypeConfiguration<ChatDiagnosisPrescription>
{
    public void Configure(EntityTypeBuilder<ChatDiagnosisPrescription> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.ChatDiagnosisId).IsRequired();

        builder.Property(p => p.Content);

        builder
            .HasOne(p => p.ChatDiagnosis)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(p => p.ChatDiagnosisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
