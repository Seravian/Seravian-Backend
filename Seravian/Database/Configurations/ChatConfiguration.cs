using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        // Table name (optional, if you want to explicitly set it)
        builder.ToTable("Chats");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Required relationship with Patient
        builder
            .HasOne(c => c.Patient)
            .WithMany(p => p.Chats) // Ensure Patient has ICollection<Chat> Chats in its class
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade); // or DeleteBehavior.Restrict if you prefer

        // One Chat has many ChatMessages
        builder
            .HasMany(c => c.ChatMessages)
            .WithOne(cm => cm.Chat)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Title is optional
        builder.Property(c => c.Title).HasMaxLength(200); // Set a reasonable max length

        // CreatedAt is required
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        // IsDeleted is required
        builder.Property(c => c.IsDeleted).IsRequired();
    }
}
