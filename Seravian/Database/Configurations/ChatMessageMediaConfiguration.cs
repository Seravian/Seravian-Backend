using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class ChatMessagem : IEntityTypeConfiguration<ChatMessageMedia>
{
    public void Configure(EntityTypeBuilder<ChatMessageMedia> builder)
    {
        // Shared PK with ChatMessage
        builder.HasKey(m => m.MessageId);

        // Properties for media-related details
        builder
            .Property(m => m.MediaType)
            .HasConversion<int>() // Store enum as string
            .IsRequired();

        builder.Property(m => m.FileName).HasMaxLength(255).IsRequired();

        builder.Property(m => m.FilePath).HasMaxLength(500).IsRequired();

        builder.Property(m => m.MimeType).HasMaxLength(100);

        builder.Property(m => m.FileSizeInBytes);

        builder.Property(m => m.Transcription);
        builder.Property(m => m.SEREmotionAnalysis);
        builder.Property(m => m.FaceAnalysis);
        builder.Property(m => m.CombinedAnalysisResult);

        // Relationship with ChatMessage (1-to-1)
        builder
            .HasOne(m => m.Message)
            .WithOne(msg => msg.Media)
            .HasForeignKey<ChatMessageMedia>(med => med.MessageId)
            .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete
    }
}
