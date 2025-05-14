using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChatVoiceAnalysisConfiguration : IEntityTypeConfiguration<ChatVoiceAnalysis>
{
    public void Configure(EntityTypeBuilder<ChatVoiceAnalysis> builder)
    { // Shared primary key and foreign key to ChatMessage
        builder.HasKey(v => v.MessageId);

        builder.Property(v => v.Transcription).HasMaxLength(2000); // Adjust as needed

        builder.Property(v => v.SEREmotionAnalysis).HasMaxLength(2000); // Adjust as needed

        builder
            .HasOne(v => v.Message)
            .WithOne(m => m.VoiceAnalysis) // Ensure ChatMessage has a VoiceAnalysis nav prop
            .HasForeignKey<ChatVoiceAnalysis>(v => v.MessageId)
            .OnDelete(DeleteBehavior.Cascade); // Optional: cascade del
    }
}
