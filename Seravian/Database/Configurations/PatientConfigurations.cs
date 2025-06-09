using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class PatientConfigurations : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(d => d.UserId); // UserId cannot be null;

        builder
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Patient>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with Chat
        builder
            .HasMany(p => p.Chats)
            .WithOne(c => c.Patient)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
