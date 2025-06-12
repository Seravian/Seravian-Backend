using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChatDiagnosisConfiguration : IEntityTypeConfiguration<ChatDiagnosis>
{
    public void Configure(EntityTypeBuilder<ChatDiagnosis> builder)
    {
        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Id).ValueGeneratedOnAdd(); // Identity

        builder.Property(cd => cd.ChatId).IsRequired();

        builder.Property(cd => cd.Description).HasMaxLength(10000);

        builder
            .Property(cd => cd.RequestedAtUtc)
            .IsRequired()
            .HasConversion(UtcDateTimeConverter.DateTimeConverter);

        builder
            .Property(cd => cd.CompletedAtUtc)
            .HasConversion(UtcDateTimeConverter.NullableDateTimeConverter);

        builder.Property(cd => cd.StartMessageId).IsRequired();

        builder.Property(cd => cd.ToMessageId).IsRequired();

        // Relationships
        builder
            .HasOne(cd => cd.Chat)
            .WithMany(c => c.ChatDiagnoses)
            .HasForeignKey(cd => cd.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cd => cd.StartMessage)
            .WithMany() // optional: .WithMany(m => m.StartedDiagnoses)
            .HasForeignKey(cd => cd.StartMessageId)
            .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete

        builder
            .HasOne(cd => cd.ToMessage)
            .WithMany() // optional: .WithMany(m => m.EndedDiagnoses)
            .HasForeignKey(cd => cd.ToMessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
