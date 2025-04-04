using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired().HasMaxLength(150); // Enforces a maximum length of 150 characters for email

        // Set constraints for FirstName and LastName
        builder
            .Property(u => u.FullName)
            .IsRequired() // First name cannot be null
            .HasMaxLength(100); // Set a maximum length for first name

        builder
            .Property(u => u.Email)
            .IsRequired() // First name cannot be null
            .HasMaxLength(100); // Set a maximum length for first name

        builder.Property(u => u.Role).HasConversion<int>().IsRequired();
        builder
            .HasIndex(u => u.Email) // Ensure email is unique
            .IsUnique();
    }
}
