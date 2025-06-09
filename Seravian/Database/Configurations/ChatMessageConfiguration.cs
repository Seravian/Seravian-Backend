using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        // Table name (optional, if you want to explicitly set it)
        builder.ToTable("ChatsMessages");
        builder.Property(cm => cm.MessageType).HasConversion<int>().IsRequired();

        // Primary Key
        builder.HasKey(cm => cm.Id);

        // Make Id an identity (auto-increment)
        builder.Property(cm => cm.Id).ValueGeneratedOnAdd(); // This will make the Id auto-incrementing

        // ChatId as Foreign Key
        builder
            .HasOne(cm => cm.Chat)
            .WithMany(c => c.ChatMessages) // Ensure Chat has ICollection<ChatMessage> ChatMessages in its class
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade); // or DeleteBehavior.Restrict if you prefer

        // Relationship with ChatMessageMedia (1-to-1)
        builder
            .HasOne(m => m.Media)
            .WithOne(med => med.Message)
            .HasForeignKey<ChatMessageMedia>(med => med.MessageId)
            .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete when ChatMessage is deleted

        // Relationship with ChatVoiceAnalysis (1-to-1)
        builder
            .HasOne(m => m.VoiceAnalysis)
            .WithOne(va => va.Message)
            .HasForeignKey<ChatVoiceAnalysis>(va => va.MessageId)
            .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete when C
        // Content is required and has a max length (for storage optimization)
        builder.Property(cm => cm.Content).IsRequired().HasMaxLength(1000); // Adjust max length as necessary

        // Time is required
        builder.Property(cm => cm.TimestampUtc).IsRequired();

        // IsAI and IsDeleted are required
        builder.Property(cm => cm.IsAI).IsRequired();

        builder.Property(cm => cm.IsDeleted).IsRequired();
    }
}
